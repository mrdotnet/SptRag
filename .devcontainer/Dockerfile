FROM python:3.13.3-slim-bookworm

# Install dependencies
RUN apt-get update && \
    apt-get install -y \
    build-essential \
    libpq-dev \
    git \
    curl \
    libreadline-dev \
    zlib1g-dev \
    libssl-dev \
    flex \
    bison \
    pkg-config \
    wget \
    postgresql-15 \
    postgresql-server-dev-15 \
    postgresql-contrib-15 \
    unzip && \
    rm -rf /var/lib/apt/lists/*

# Upgrade pip to latest version
RUN pip install --upgrade pip

# Python dependencies
COPY requirements.txt .

# Apache AGE
RUN git clone https://github.com/apache/age.git /tmp/age && \
    cd /tmp/age && \
    git checkout release/PG15/1.4.0 && \
    make PG_CONFIG=/usr/lib/postgresql/15/bin/pg_config && \
    make install PG_CONFIG=/usr/lib/postgresql/15/bin/pg_config

# pgvector
RUN git clone https://github.com/pgvector/pgvector.git /tmp/pgvector && \
    cd /tmp/pgvector && \
    make && make install

# Setup workspace
WORKDIR /workspace

