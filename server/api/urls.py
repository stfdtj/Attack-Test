from django.urls import path
from . import views

urlpatterns = [
    path("ping/", views.ping),
    path("attack/", views.attack),
    path("loot/claim/", views.loot_claim),
    path("inventory/", views.inventory),
]