function initSignInForm() {
    let challenge = uuidv4();
    document.querySelectorAll(".terminal").forEach(terminal => {
        terminal.value = terminal.value.replace(/{challenge}/g, challenge);
        terminal.value = terminal.value.replace("https://app.gaevoy.com/keybase-signin-demo", "http://localhost:5002");
    });

    fetch('session/' + challenge, {method: "POST"})
        .then(() => fetch('session'))
        .then(resp => resp.json())
        .then(resp => {
            console.log(resp);
            if (resp.isAuthenticated)
                return fetch('https://keybase.io/_/api/1.0/user/lookup.json?usernames=' + resp.name);
        })
        .then(resp => resp.json())
        .then(resp => {
            console.log(resp);
        });
}

function uuidv4() {
    return ([1e7] + [1e3] + [4e3] + [8e3] + [1e11]).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    )
}