namespace ZeldaArena.Web.Payments;

/// <summary>Куда отправить покупателя после отправки кода из письма.</summary>
public enum PaymentNextStep
{
    /// <summary>Остаться на форме кода: код неверен, но попытки ещё есть.</summary>
    StayOnForm,

    /// <summary>Подписка оформлена — страница успеха.</summary>
    SubscriptionActivated,

    /// <summary>Страница заказа: оплачен или уже был завершён раньше.</summary>
    OrderDetails,

    /// <summary>Корзина: этот запрос провалил оплату, заказ отменён, товары вернулись.</summary>
    Cart,
}