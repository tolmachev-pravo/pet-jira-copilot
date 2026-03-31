# Yandex OAuth integration setup

Эта инструкция нужна для локальной и повторной настройки Yandex OAuth.

## Что использует приложение

Приложение читает настройки из секции:

```json
"YandexOAuth": {
  "ClientId": "...",
  "ClientSecret": "...",
  "RedirectUri": "/profile/integrations/yandex/callback",
  "Scope": "calendar:all"
}
```

Схема конфигурации описана в [YandexOAuthConfiguration.cs](D:/Projects/Pet.Jira/src/Pet.Jira.Application/Integrations/YandexOAuthConfiguration.cs).

Важно:
- в конфиге приложения `RedirectUri` можно хранить как относительный path
- в запрос к Yandex приложение всё равно отправляет абсолютный URL
- абсолютный callback URL должен быть заранее зарегистрирован в кабинете Yandex OAuth

## Где взять ClientId и ClientSecret

1. Откройте [Yandex OAuth](https://oauth.yandex.com/).
2. Войдите под нужной учётной записью Yandex.
3. Создайте новое OAuth-приложение или откройте уже существующее.
4. В карточке приложения найдите:
   - `Client ID`
   - `Client secret`
5. Если секрет был скомпрометирован или попал в git, перевыпустите его в кабинете Yandex.

## RedirectUri для приложения

Внутри конфигурации проекта рекомендуется хранить path:

```text
/profile/integrations/yandex/callback
```

Во время запроса приложение автоматически соберёт абсолютный URL из текущего `scheme` и `host`.

Пример для локальной разработки:

```text
https://localhost:44310/profile/integrations/yandex/callback
```

Пример для production:

```text
https://your-domain/profile/integrations/yandex/callback
```

Важно:
- в кабинете Yandex должен быть зарегистрирован абсолютный URL
- если меняется локальный порт или production host, обновить нужно именно callback URL в кабинете Yandex

## Как указать секреты через user-secrets

Проект уже использует user-secrets.
`UserSecretsId` задан в [Pet.Jira.Web.csproj](D:/Projects/Pet.Jira/src/Pet.Jira.Web/Pet.Jira.Web.csproj):

```text
3a21bd1f-a675-4630-b374-69f562af1251
```

`dotnet user-secrets init` выполнять не нужно.

Из корня репозитория задайте значения так:

```powershell
dotnet user-secrets set "YandexOAuth:ClientId" "YOUR_CLIENT_ID" --project src/Pet.Jira.Web/Pet.Jira.Web.csproj
dotnet user-secrets set "YandexOAuth:ClientSecret" "YOUR_CLIENT_SECRET" --project src/Pet.Jira.Web/Pet.Jira.Web.csproj
dotnet user-secrets set "YandexOAuth:RedirectUri" "/profile/integrations/yandex/callback" --project src/Pet.Jira.Web/Pet.Jira.Web.csproj
dotnet user-secrets set "YandexOAuth:Scope" "calendar:all" --project src/Pet.Jira.Web/Pet.Jira.Web.csproj
```

Проверить сохранённые значения:

```powershell
dotnet user-secrets list --project src/Pet.Jira.Web/Pet.Jira.Web.csproj
```

## Альтернатива: environment variables

Если нужно, можно передать настройки через переменные окружения:

```text
YandexOAuth__ClientId
YandexOAuth__ClientSecret
YandexOAuth__RedirectUri
YandexOAuth__Scope
```

Для локальной разработки по умолчанию используйте именно user-secrets.

## Как проверить, что всё настроено

1. Запустите приложение.
2. Убедитесь, что startup не падает из-за отсутствия конфигурации.
3. Откройте `/profile`.
4. Нажмите `Подключить Yandex`.
5. Проверьте, что происходит redirect на страницу авторизации Yandex, а не возврат на `/profile?yandexStatus=error`.
6. Убедитесь, что `redirect_uri`, который уходит в Yandex, соответствует абсолютному URL текущего окружения.

## Важные правила безопасности

- Не храните реальные `ClientId` и особенно `ClientSecret` в `appsettings.json` и `appsettings.Development.json`.
- Не коммитьте реальные секреты в git.
- Если секрет уже был записан в репозиторий, перевыпустите `Client secret` в кабинете Yandex.
- Для локальной разработки используйте user-secrets или environment variables.

## Текущее ожидаемое значение Scope

Сейчас приложение ожидает:

```text
calendar:all
```

Это текущее значение конфигурации приложения, а не абстрактный пример. Если схема интеграции будет меняться, обновите и эту инструкцию, и локальные секреты.
