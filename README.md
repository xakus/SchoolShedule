# School Schedule Generator

Система микросервисов для автоматической генерации школьного расписания.

## Микросервисы

- **UserService** (HTTP `5000` / HTTPS `5001`): управление пользователями и аутентификация.
- **SchoolService** (HTTP `5002` / HTTPS `5003`): управление информацией о классах, расписаниях и проверках.
- **GenerationService** (HTTP `5004` / HTTPS `5005`): алгоритмическая генерация расписания на основе данных.

## Пререквизиты

- Docker & Docker Compose
- .NET SDK 8.0+
- Linux или WSL2 (для корректной работы скрипта `start.sh`)

## Установка и запуск

```bash
git clone https://github.com/xakus/SchoolShedule.git
cd SchoolShedule
chmod +x start.sh
./start.sh
```

Скрипт автоматически:
- Проверит и установит Docker, Docker Compose и .NET SDK.
- Создаст или запустит контейнеры PostgreSQL и Redis.
- Остановит, пересоберёт и запустит все микросервисы.
- Настроит автозапуск `start.sh` при перезагрузке системы.
- Инициализирует базу данных `school_db` внутри контейнера.

## Использование

После запуска сервисы доступны через Swagger UI:
- UserService: `http://localhost:5000/swagger/index.html`
- SchoolService: `http://localhost:5002/swagger/index.html`
- GenerationService: `http://localhost:5004/swagger/index.html`

## Переменные окружения

Конфигурация задаётся в файлах:
- `docker_files/docker-compose.yml`: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `REDIS_PASSWORD`.
- `appsettings.json` каждого сервиса: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`.

## Поддержка

Возникли вопросы или проблемы? Открывайте issue в репозитории.
