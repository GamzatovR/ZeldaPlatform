namespace ZeldaArena.Application.Features.Admin.Orders.Commands.ChangeOrderStatus;

/// <summary>Что делает администратор с заказом: отправляет, завершает или отменяет.</summary>
public enum OrderTransition
{
    Ship = 0,
    Complete = 1,
    Cancel = 2,
}