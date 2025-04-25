#!/bin/bash

# === НАСТРОЙКИ ДЛЯ ЗАПОЛНЕНИЯ ===

# Пользователь Linux (обычно $USER, но можно явно указать)
LINUX_USER="$USER"


# Имя контейнера Postgres (должно совпадать с docker-compose)
CONTAINER_NAME="postgres"
PGUSER="postgres"
PGPASSWORD="DBPASSWORD"
DBNAME="school_db"
# Ветка для клонирования (обычно main или master)
GIT_BRANCH="master"

# === ДАЛЬШЕ КОД СКРИПТА ===

set -e

echo "Проверка наличия Docker..."
if ! command -v docker &> /dev/null; then
    echo "Устанавливаю Docker..."
    sudo apt update
    sudo apt install -y apt-transport-https ca-certificates curl software-properties-common
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
    sudo apt update
    sudo apt install -y docker-ce
    sudo usermod -aG docker $LINUX_USER
    newgrp docker
fi

echo "Проверка наличия Docker Compose..."
if ! command -v docker-compose &> /dev/null; then
    echo "Устанавливаю Docker Compose..."
    sudo apt install -y docker-compose
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCKER_DIR="$SCRIPT_DIR/docker_files"
DOCKER_COMPOSE_FILE="$DOCKER_DIR/docker-compose.yml"

cd "$DOCKER_DIR"
if [ -f "$DOCKER_COMPOSE_FILE" ]; then
    echo "Проверка статуса docker-compose..."
    if ! docker-compose ps | grep -q postgres; then
        echo "Запуск docker-compose сервисов..."
        docker-compose up -d
    else
        echo "Docker Compose сервисы уже запущены."
    fi
else
    echo "Файл docker-compose.yml не найден!"
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo "Устанавливаю .NET SDK..."
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    sudo apt update
    sudo apt install -y dotnet-sdk-8.0
fi

for service in UserService SchoolService GenerationService; do
    SERVICE_DIR="$SCRIPT_DIR/$service"
    cd "$SERVICE_DIR"
    echo "Выполняю dotnet restore для $service..."
    dotnet restore
    echo "Собираю $service..."
    dotnet build --configuration Release
    echo "Запускаю $service..."
    nohup dotnet run --configuration Release > "$service.log" 2>&1 &
done

# === ИНИЦИАЛИЗАЦИЯ БАЗЫ SCHOOL_DB В КОНТЕЙНЕРЕ DOCKER ===



SQLFILE="$DOCKER_DIR/postgres_init/start.sql"

# Проверить, запущен ли контейнер
if docker ps | grep -q "$CONTAINER_NAME"; then
  echo "PostgreSQL контейнер $CONTAINER_NAME запущен. Проверяем наличие базы данных $DBNAME..."

  DB_EXISTS=$(docker exec -e PGPASSWORD="$PGPASSWORD" $CONTAINER_NAME psql -U "$PGUSER" -tAc "SELECT 1 FROM pg_database WHERE datname='$DBNAME';")
  if [ "$DB_EXISTS" != "1" ]; then
    echo "База данных $DBNAME не существует. Создаём..."
    docker exec -e PGPASSWORD="$PGPASSWORD" $CONTAINER_NAME createdb -U "$PGUSER" "$DBNAME"
  else
    echo "База данных $DBNAME уже существует."
  fi

  echo "Копируем скрипт start.sql в контейнер и выполняем..."
  docker cp "$SQLFILE" $CONTAINER_NAME:/start.sql
  docker exec -e PGPASSWORD="$PGPASSWORD" $CONTAINER_NAME psql -U "$PGUSER" -d "$DBNAME" -f /start.sql
else
  echo "Контейнер PostgreSQL не запущен. Скрипт будет выполнен автоматически при первом запуске контейнера."
fi

echo "Все сервисы запущены!"