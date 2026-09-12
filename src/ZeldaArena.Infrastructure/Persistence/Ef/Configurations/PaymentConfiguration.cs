using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class PaymentConfiguration : EntityConfiguration<Payment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        // Столбцов под полный номер карты и CVV в схеме нет и не будет.
        builder.Property(payment => payment.CardLast4).IsRequired().HasMaxLength(4).IsFixedLength();
        builder.Property(payment => payment.CardBrand).IsRequired().HasMaxLength(20);
        builder.Property(payment => payment.ConfirmationEmail).IsRequired().HasMaxLength(256);
        builder.Property(payment => payment.ConfirmationCodeHash).IsRequired().HasMaxLength(128);
        builder.Property(payment => payment.IdempotencyKey).IsRequired().HasMaxLength(80);
        builder.Property(payment => payment.FailureReason).HasMaxLength(200);

        builder.Ignore(payment => payment.IsPending);

        builder.ComplexProperty(payment => payment.Amount, money =>
        {
            money.Property(amount => amount.Amount).HasColumnName("Amount");
            money.Property(currency => currency.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsFixedLength();
        });

        // Повторная отправка формы оплаты не должна создавать второй платёж.
        builder.HasIndex(payment => payment.IdempotencyKey).IsUnique();
        builder.HasIndex(payment => new { payment.UserId, payment.Status });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(payment => payment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Subscription>()
            .WithMany()
            .HasForeignKey(payment => payment.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}