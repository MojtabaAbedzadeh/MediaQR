namespace MediaQR.Application.Interfaces;

public interface IMediaUrlSigner
{
    string CreateSignature(
        string code,
        DateTimeOffset expiresAt);

    bool ValidateSignature(
        string code,
        DateTimeOffset expiresAt,
        string signature);
}