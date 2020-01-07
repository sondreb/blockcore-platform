using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform
{
    public class Identity
    {
        Mnemonic mnemonic;
        ExtKey masterNode;
        ExtKey identity;
        PubKey identityPubKey;

        public Identity(int index = 0) : this(new Mnemonic(Wordlist.English, WordCount.Twelve), index)
        {

        }

        public Identity(string recoveryPhrase, int index = 0) : this(new Mnemonic(recoveryPhrase, Wordlist.English), index)
        {

        }

        public Identity(Mnemonic mnemonic, int index = 0)
        {
            Index = index;

            // This means we'll keep the recovery phrase in-memory for the lifetime of the process. This is needed for identity and
            // hub communication, but we should in the future only derive the extkey from the purpose (302) of identity, and then we should
            // use the extpubkey to derive wallet addresses (purpose 44). If we want to trust users to use the same recovery phrase for
            // both wallets and hub/identity, we must ensure that the private keys for the wallet is as secure as possible.
            this.mnemonic = mnemonic;

            this.masterNode = mnemonic.DeriveExtKey();
            
            // We should persist the extpubkey from the wallet root, and
            // persist the extkey for identity. Both must be stored encrypted on disk, either
            // using a password from the user, or by relying on TPM module, etc.
            // this.walletRoot = masterNode.Derive(new KeyPath("m/44'")).Neuter();
            // this.identityRoot = masterNode.Derive(new KeyPath("m/302'"));

            this.identity = masterNode.Derive(new KeyPath("m/302'")).Derive(index, true);
            // this.identity = masterNode.Derive(new KeyPath("m/302'/0'"));

            // The default for keys is to be compressed, making it 33 bytes as oppose to 65.
            this.identityPubKey = this.identity.GetPublicKey();

            this.FingerPrint = this.identityPubKey.GetHDFingerPrint();
            this.Id = this.identityPubKey.Hash.ToString();

            var test = this.identityPubKey.ToHex();

            var keyId = new KeyId(Id);

            Base58Data data = new Base58Data();
            data.

            var test2 = keyId.ScriptPubKey.ToHex();
        }

        public string Decrypt(string data)
        {
            var decryptedMessage = identity.PrivateKey.Decrypt(data);
            return decryptedMessage;
        }

        public Identity GetIdentity(int index)
        {
            return new Identity(this.mnemonic, index);
        }

        public HDFingerprint FingerPrint { get; }

        public string Id { get; }

        public int Index { get; set; }
    }
}
