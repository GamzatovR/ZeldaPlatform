namespace ZeldaArena.Application.Common.Interfaces;

public interface IHtmlSanitizer
{
    string Sanitize(string html);
}