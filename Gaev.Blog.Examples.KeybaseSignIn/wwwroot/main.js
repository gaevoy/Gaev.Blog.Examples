function initSignInForm() {
    let challenge = uuidv4();
    document.querySelectorAll(".terminal").forEach(terminal => {
        terminal.value = terminal.value.replace(/{challenge}/g, challenge);
        terminal.value = terminal.value.replace("https://app.gaevoy.com/keybase-signin-demo", "http://localhost:5002");
    });

    let api = new SignInApi();
    api.waitForSignedChallenge(challenge, signed => {
        console.log(signed);
    });
}

class SignInApi {
    waitForSignedChallenge(challenge, onMessageReceived) {
        let source = new EventSource('api/' + challenge);
        source.addEventListener('message', evt => {
            onMessageReceived(JSON.parse(evt.data));
            source.close();
        });
    }
}

class PgpKey {
    constructor(key) {
        this._key = key;
        if (this.canDecrypt()) {
            this._ring = new kbpgp.keyring.KeyRing();
            this._ring.add_key_manager(key);
        }
    }

    public() {
        return this._key.armored_pgp_public;
    }

    id() {
        return this._key.get_pgp_short_key_id();
    }

    nickname() {
        return this._key.userids[0].get_username();
    }

    encrypt(text, onDone) {
        kbpgp.box({msg: text, encrypt_for: this._key}, (_, cipher) =>
            onDone(cipher));
    }

    canDecrypt() {
        return this._key.can_decrypt();
    }

    decrypt(cipher, onDone) {
        kbpgp.unbox({keyfetch: this._ring, armored: cipher, progress_hook: null}, (_, literals) =>
            onDone(literals[0].toString()));
    }

    static generate(nickname, onDone) {
        let opt = {userid: nickname, primary: {nbits: 1024}, subkeys: []};
        kbpgp.KeyManager.generate(opt, (_, key) =>
            key.sign({}, () =>
                key.export_pgp_public({}, () =>
                    onDone(new PgpKey(key)))));
    }

    static load(publicKey, onDone) {
        kbpgp.KeyManager.import_from_armored_pgp({armored: publicKey}, (_, key) =>
            onDone(new PgpKey(key)));
    }
}

function uuidv4() {
    return ([1e7] + [1e3] + [4e3] + [8e3] + [1e11]).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    )
}