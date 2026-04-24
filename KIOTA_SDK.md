# Использование SDK (Microsoft Kiota)

Kiota — это современный генератор SDK от Microsoft, который создает легкие, модульные и строго типизированные клиенты. 

## 🏗️ Преимущества перед обычным Retrofit:
1.  **Fluent API**: Вызовы строятся цепочкой `client.api().auth().login().post(request)`.
2.  **Типизация ошибок**: Каждая ошибка (400, 401, 404) автоматически превращается в конкретный Java-класс с вашим Enum кодом ошибки.
3.  **Легкость**: Не тянет за собой лишних зависимостей, если они не нужны.

---

## ☕ Java SDK

### 1. Подключение зависимостей (Gradle)
Для работы SDK в Java-проект нужно добавить стандартные библиотеки Kiota:
```gradle
dependencies {
    implementation 'com.microsoft.kiota:microsoft-kiota-abstractions:1.1.0'
    implementation 'com.microsoft.kiota:microsoft-kiota-http-ok-http:1.1.0'
    implementation 'com.microsoft.kiota:microsoft-kiota-serialization-json:1.1.0'
    implementation 'com.microsoft.kiota:microsoft-kiota-serialization-text:1.1.0'
    implementation 'com.microsoft.kiota:microsoft-kiota-serialization-form:1.1.0'
}
```

### 2. Пример использования (Result pattern через try-catch)
Kiota автоматически маппит ваши ошибки из OpenAPI на конкретные классы.

```java
import driprate.sdk.ApiClient;
import com.microsoft.kiota.authentication.AnonymousAuthenticationProvider;
import com.microsoft.kiota.http.OkHttpRequestAdapter;

// 1. Настройка (делается один раз)
var authProvider = new AnonymousAuthenticationProvider();
var adapter = new OkHttpRequestAdapter(authProvider);
var client = new ApiClient(adapter);

// 2. Вызов
try {
    var request = new RegisterRequest();
    request.setEmail("test@test.com");
    request.setPassword("secure123");
    
    // Fluent API!
    var userId = client.api().auth().register().post(request);
    System.out.println("Успешно создан пользователь: " + userId);

} catch (BadRequestError e) {
    // ВАШИ КОДЫ ОШИБОК ТУТ!
    // Благодаря вашему ErrorCodesTransformer, это типизировано!
    var errorCode = e.getPrimaryErrorMessage(); // или e.getCode() в зависимости от маппинга
    System.err.println("Ошибка 400: " + errorCode);
} catch (UnauthorizedError e) {
    System.err.println("Ошибка 401: Неверные учетные данные");
}
```

---

## 🟦 TypeScript SDK

### 1. Подключение
```bash
npm install @microsoft/kiota-abstractions @microsoft/kiota-http-fetchlibrary @microsoft/kiota-serialization-json @microsoft/kiota-serialization-text
```

### 2. Пример использования
```typescript
import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
import { ApiClient } from './sdks/ts-kiota/apiClient';

const adapter = new FetchRequestAdapter(new AnonymousAuthenticationProvider());
const client = new ApiClient(adapter);

async function register() {
    try {
        const response = await client.api.auth.register.post({
            email: "test@example.com",
            password: "password123"
        });
        console.log("User ID:", response);
    } catch (error) {
        // Kiota бросает типизированные ошибки
        if (error instanceof BadRequestError) {
             console.log("Error Code:", error.code); // Enum!
        }
    }
}
```

---

## 🛠️ Как обновлять SDK
Для обновления SDK после изменения API используйте:
```bash
kiota generate -l java -d openapi.json -o sdks/java-kiota -n DripRate.Sdk
kiota generate -l typescript -d openapi.json -o sdks/ts-kiota -n DripRate.Sdk
```
