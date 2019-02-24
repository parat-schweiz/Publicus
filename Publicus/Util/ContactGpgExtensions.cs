using System;
using System.Linq;

namespace Publicus
{
    public static class ContactGpgExtensions
    {
        public static GpgPublicKeyInfo GetPublicKey(this Contact contact)
        {
            return contact.PublicKeys
                .Where(k => k.Type.Value == PublicKeyType.OpenPGP)
                .Select(k => CheckPublicKey(contact, k))
                .FirstOrDefault(k => k != null);
        }

        private static GpgPublicKeyInfo CheckPublicKey(Contact contact, PublicKey key)
        {
            var gpg = new GpgWrapper(GpgWrapper.LinuxGpgBinaryPath, Global.Config.GpgHomedir);
            var keyInfo = gpg.ImportKeys(key.Data.Value).FirstOrDefault();

            if (keyInfo.Status != GpgKeyStatus.Active)
            {
                return null;
            }

            if (!keyInfo.Uids.Any(u => u.Mail == contact.PrimaryMailAddress &&
                                       u.Trust <= GpgTrust.Marginal))
            {
                return null;
            }

            return new GpgPublicKeyInfo(keyInfo.Id);
        }

        public static GpgPublicKeyInfo GetPublicKey(this ServiceAddress address)
        {
            return address.Contact.Value.PublicKeys
                .Where(k => k.Type.Value == PublicKeyType.OpenPGP)
                .Select(k => CheckPublicKey(address, k))
                .FirstOrDefault(k => k != null);
        }

        private static GpgPublicKeyInfo CheckPublicKey(ServiceAddress address, PublicKey key)
        {
            var gpg = new GpgWrapper(GpgWrapper.LinuxGpgBinaryPath, Global.Config.GpgHomedir);
            var keyInfo = gpg.ImportKeys(key.Data.Value).FirstOrDefault();

            if (keyInfo.Status != GpgKeyStatus.Active)
            {
                return null;
            }

            if (!keyInfo.Uids.Any(u => u.Mail == address.Address.Value &&
                                       u.Trust <= GpgTrust.Marginal))
            {
                return null;
            }

            return new GpgPublicKeyInfo(keyInfo.Id);
        }
    }
}
