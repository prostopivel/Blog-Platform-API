#!/bin/bash

set -e

echo "Starting database initialization..."

wait_for_postgres() {
    local host=$1
    local port=$2
    local db_name=$3
    echo "Waiting for PostgreSQL on $host:$port ($db_name) to be ready..."

    until pg_isready -h $host -p $port -U postgres; do
        echo "PostgreSQL on $host:$port is unavailable - sleeping"
        sleep 2
    done

    echo "PostgreSQL on $host:$port is ready"
}

init_database() {
    local host=$1
    local port=$2
    local db_name=$3
    local script_path=$4

    echo "Initializing database $db_name on $host:$port..."

    if ! psql -h $host -p $port -U postgres -lqt | cut -d \| -f 1 | grep -qw $db_name; then
        echo "Creating database $db_name..."
        createdb -h $host -p $port -U postgres $db_name
    else
        echo "Database $db_name already exists"
    fi

    if [ -f "$script_path" ]; then
        echo "Executing SQL script: $script_path"
        psql -h $host -p $port -U postgres -d $db_name -f "$script_path"
        echo "Database $db_name initialized successfully"
    else
        echo "SQL script not found: $script_path"
        exit 1
    fi
}

wait_for_postgres "auth-db" "5432" "auth_db"
wait_for_postgres "blog-db" "5432" "blog_db" 

init_database "auth-db" "5432" "auth_db" "/app/init-scripts/auth-init.sql"
init_database "blog-db" "5432" "blog_db" "/app/init-scripts/blog-init.sql"

echo "All databases initialized successfully!"