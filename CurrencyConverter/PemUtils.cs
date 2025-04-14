using System.Security.Cryptography;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;

public static class PemUtils
{
    public static RsaSecurityKey LoadPrivateKey(string pemPath)
    {
        using var reader = File.OpenText(pemPath);
        var pemReader = new PemReader(reader);
        var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
        var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);
        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParams);
        return new RsaSecurityKey(rsa);
    }

    public static RsaSecurityKey LoadPublicKey(string pemPath)
    {
        using var reader = File.OpenText(pemPath);
        var pemReader = new PemReader(reader);
        var certObject = pemReader.ReadObject();

        if (certObject is Org.BouncyCastle.X509.X509Certificate certificate)
        {
            var keyParams = (RsaKeyParameters)certificate.GetPublicKey();
            var rsaParams = DotNetUtilities.ToRSAParameters(keyParams);
            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);
            return new RsaSecurityKey(rsa);
        }
        else if (certObject is RsaKeyParameters publicKey)
        {
            var rsaParams = DotNetUtilities.ToRSAParameters(publicKey);
            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);
            return new RsaSecurityKey(rsa);
        }

        throw new InvalidOperationException("Unsupported PEM format. Expected certificate or RSA public key.");
    }

}
