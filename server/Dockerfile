FROM python:3.11-slim

# working directory
WORKDIR /app

# system utilities 
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential git curl vim && \
    rm -rf /var/lib/apt/lists/*

# upgrade pip and set Python environment
RUN python -m pip install --upgrade pip wheel setuptools


RUN pip install \
    django djangorestframework gunicorn fastapi uvicorn[standard] python-dotenv

# Expose a default dev port
EXPOSE 8000

CMD ["bash"]