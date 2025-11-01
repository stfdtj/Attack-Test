import json, random, uuid, threading
from django.http import JsonResponse, HttpResponseBadRequest
from django.views.decorators.http import require_POST, require_GET


# Create your views here.


# disposable server
# one time only



def ping(request):
    return JsonResponse({"ok": True})

LOCK = threading.Lock()

# Players: {id: {"name":...}}
PLAYERS = {1: {"name": "Player1"}}

# Attackable object: {id: {"name":..., "type":..., "hp":..., "max_hp":..., "alive":bool, "drop_table":[...]}}
ENTITIES = {
    1: {
        "name": "Fir_tree",
        "type": "Tree",
        "hp": 100,
        "max_hp": 100,
        "alive": True,
        "drop_table": [
            {"name": "AppleGreen", "p": 1, "min": 1, "max": 2},
        ],
    },
    2: {
        "name": "BoxReady",
        "type": "Box",
        "hp": 25,
        "max_hp": 25,
        "alive": True,
        "drop_table": [
            {"name": "Gem", "p": 1, "min": 0, "max": 1},
        ],
    },
    3: {
        "name": "Spider",
        "type": "Spider",
        "hp": 50,
        "max_hp": 50,
        "alive": True,
        "drop_table": [
            {"name": "SteakCooked", "p": 1, "min": 1, "max": 2},
        ],
    },
    4: {
        "name": "Steve",
        "type": "NPC",
        "hp": 200,
        "max_hp": 200,
        "alive": True,
        "drop_table": [
            {"name": "Carrot", "p": 1, "min": 1, "max": 3},
        ],
    },
}

# Inventory: {player_id: {item_id: qty}}
INVENTORY = {1: {}}

EVENTS = {}


def json_from_request(request):
    """Parse incoming request body into a dict.

    Accepts JSON (preferred) or form-encoded bodies (via request.POST or parse_qs).
    Returns an empty dict on failure.
    """
    try:
        body = request.body.decode("utf-8")
    except Exception:
        body = ""

    if body:
        # try JSON first
        try:
            return json.loads(body)
        except Exception:
            # try Django form data (QueryDict)
            try:
                return {k: v for k, v in request.POST.items()}
            except Exception:
                # fallback: parse query-style body
                try:
                    from urllib.parse import parse_qs

                    qs = parse_qs(body)
                    # convert lists to single values when appropriate
                    return {k: v[0] if len(v) == 1 else v for k, v in qs.items()}
                except Exception:
                    return {}

    return {}
    
def serialize_inventory(player_id):
    bag = INVENTORY.get(player_id, {})
    return [{"item_name": k, "qty": v} for k, v in sorted(bag.items())]

# decide loot
def roll_loot(rng, drop_table):
    out = []
    for r in drop_table:
        if rng.random() <= float(r.get("p", 0)):
            qty = rng.randint(int(r.get("min", 1)), int(r.get("max", 1)))
            if qty > 0:
                out.append({"item_name": r["name"], "qty": qty})
    return out



encounter_id = 0
@require_POST
def attack(request):
    """
    { "player_id": int, "target_id": int }
    which player is attacking and its target
    """
    data = json_from_request(request)
    # ensure we update the global encounter counter
    global encounter_id
    try:
        player_id = int(data["player_id"])
        target_id = int(data["target_id"])
    except Exception:
        return HttpResponseBadRequest("invalid body")

    with LOCK:
        # minimal existence checks
        if player_id not in PLAYERS:
            return HttpResponseBadRequest("unknown player")
        if target_id not in ENTITIES:
            return HttpResponseBadRequest("unknown target")

        target = ENTITIES[target_id]
        # if dead reply and expect loot claim
        if not target["alive"]:
            resp = {"hit": False, "damage": 0, "target_hp": target["hp"], "dead": True}
            return JsonResponse(resp)

        
        # else do attack
        hit = True
        dmg = 25

        target["hp"] = target["hp"] - dmg

        dead = target["hp"] <= 0
        if dead:
            target["alive"] = False

        # record this event
        EVENTS[encounter_id] = {
            "player_id": player_id, "target_id": target_id,
            "dead": dead, "loot_claimed": False
        }

        # respond with attack result
        resp = {
            "hit": hit, "damage": dmg, "target_hp": target["hp"],
            "dead": dead, "encounter_id": encounter_id
        }

        encounter_id += 1
    return JsonResponse(resp)

@require_POST
def loot_claim(request):
    """
    Body: { "player_id": int, "target_id": int, "encounter_id": int }
    """
    data = json_from_request(request)
    try:
        player_id = int(data["player_id"])
        target_id = int(data["target_id"])
        encounter_id = int(data["encounter_id"])
    except Exception:
        return HttpResponseBadRequest("invalid body")


    with LOCK:
        event = EVENTS.get(encounter_id)
        # check event existence and validity
        if not event or event["player_id"] != player_id or event["target_id"] != target_id:
            return HttpResponseBadRequest("invalid encounter")
        # check status
        if not event["dead"]:
            return HttpResponseBadRequest("target not dead")

        # if already claimed, return empty loot
        if event["loot_claimed"]:
            return JsonResponse({"loot": [], "inventory": serialize_inventory(player_id)})

        # else give loot
        target = ENTITIES[target_id]
        rng = random.Random()
        loot = roll_loot(rng, target.get("drop_table", []))

        # put it to inventory and respond
        bag = INVENTORY.setdefault(player_id, {})
        for item in loot:
            # loot items are returned as {"item_name": ..., "qty": ...}
            name = item.get("item_name") or item.get("name")
            if not name:
                continue
            bag[name] = bag.get(name, 0) + int(item.get("qty", 0))

        event["loot_claimed"] = True

        return JsonResponse({"loot": loot, "inventory": serialize_inventory(player_id)})


@require_POST
def inventory(request):
    data = json_from_request(request)
    try:
        player_id = int(data["player_id"])
    except Exception:
        return HttpResponseBadRequest("invalid body")
   
    if player_id not in PLAYERS:
        return HttpResponseBadRequest("unknown player")
    return JsonResponse(serialize_inventory(player_id), safe=False)
