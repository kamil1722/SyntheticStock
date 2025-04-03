# Стек:
1. .NET Aspire 
2. PostgreSQL
3. RabbitMQ 
4. Микросервисная архитектура
5. Web Api

# Описание
У нас есть три основных проекта
1. [WebApi](WebApi) - он получает данные от клиента, отправляет их микросервису. Полученный результат из микросервиса передает брокеру сообщений
2. [FutureService](Microservices/FuturesService)(использует пакет Binance.Net) обрабатывает параметры полученные из API, возвращает ему результат
3. [DataWorkService](DataWorkService) получает данные из брокера сообщений, сохраняет в базу данных

# В чем преимущество архитектуры в моем проекте? 
1. Слабая связанность. Можно реализовать несколько микросервисов для работы с биржами. Клиент и сервер работают отдельно, после отправки сообщения брокеру work service обрабатывает его, не блокируя основной поток приложения
2. Внедрение .NET Aspire. Позволяет управлять проектами(оркестрация, управление параметрами ресурсов, упрощенная контейниризация). Управление ресурсами позволяет, такие параметры как порты, имена пользователей и тд объявлять в одном  [файле](Aspire/SyntheticStockAspire.AppHost/EnvironmentSetup.cs)

# Скрипт таблицы PostgreSQL
```

CREATE TABLE IF NOT EXISTS public."FuturesPriceDifferences"
(
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    symbol1 text COLLATE pg_catalog."default" NOT NULL,
    symbol2 text COLLATE pg_catalog."default" NOT NULL,
    "time" timestamp with time zone NOT NULL,
    difference numeric NOT NULL,
    "interval" text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "PK_FuturesPriceDifferences" PRIMARY KEY (id)
)

```
# Инструкция по применению 
запустить SyntheticStockAspire.AppHost , выбираем webapi

Формат для ввода данных:
1. symbol: BTCUSDT, ETCUSDT, и тд
2. time: 2025-01-01T00:00:00
3. interval: 1m, 5m, 15m, 30m, 1h, 4h. 1d, 1w, 1M
