# IdempotentApiDemo

Bu proje, API çağrılarında idempotentliği sağlayan bir ASP.NET Core uygulamasıdır. İdempotent işlemler, aynı isteğin birden fazla kez gönderildiğinde bile yan etkilerin yalnızca bir kez oluşmasını sağlar.

## Özellikler

- API isteklerinde idempotent davranışı uygulayan özel bir öznitelik (`[Idempotent]`)
- İki farklı idempotent davranış stratejisi:
    - `ReturnFromCache`: Tekrar eden isteklerde önbellekten yanıt döner
    - `ThrowErrorIfExists`: Tekrar eden isteklerde hata fırlatır

## Proje Yapısı

### Controllers

- **OrdersController**: Sipariş işlemleri için API endpoint'leri
    - `POST /api/Orders`: Standart idempotent sipariş oluşturma
    - `POST /api/Orders/strict`: Katı idempotent kontrolü olan sipariş oluşturma
    - `GET /api/Orders/{id}`: ID'ye göre sipariş bilgisi sorgulama

### İdempotent Mekanizması

`[Idempotent]` özniteliği, gelen istekleri işlerken:
1. İstek için benzersiz bir anahtar oluşturur
2. Aynı anahtar ile daha önce işlenmiş istek varsa, belirlenen stratejiye göre yanıt verir
3. Yeni istekleri işler ve sonuçları önbelleğe alır

## Kullanım

API'yi çağırırken istemci, tekrarlanabilirliği sağlamak için idempotency-key başlığı ile benzersiz bir tanımlayıcı gönderir. Aynı anahtar ile yapılan tekrar çağrılarda:

- Standart endpoint (`/api/Orders`) aynı sonucu önbellekten döndürür
- Katı endpoint (`/api/Orders/strict`) hata fırlatır

## Kullanım Senaryoları

- Ödeme işlemleri
- Sipariş kayıtları
- Ağ kesintilerinde güvenli işlem tekrarı
- Çift tıklama ve yinelenen form gönderilerini önleme

Bu proje, kritik işlemlerde veri bütünlüğünü ve tutarlılığını sağlamak için idempotentlik ilkesinin nasıl uygulanabileceğini göstermektedir.