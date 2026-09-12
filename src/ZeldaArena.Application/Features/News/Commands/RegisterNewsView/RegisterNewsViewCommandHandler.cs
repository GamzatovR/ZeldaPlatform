using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.News.Commands.RegisterNewsView;

public sealed class RegisterNewsViewCommandHandler(INewsRepository news, IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterNewsViewCommand, Result>
{
    public async Task<Result> Handle(RegisterNewsViewCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var article = await news.GetByIdAsync(request.ArticleId, cancellationToken).ConfigureAwait(false);

        if (article is not { IsPublished: true })
        {
            return Result.Success();
        }

        article.RegisterView();

        await news.UpdateAsync(article, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}