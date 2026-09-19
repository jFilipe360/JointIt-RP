// Please see documentation at
// https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const notificacaoCount =
    document.getElementById("notificacaoCount");

const dropdownNotificacoes =
    document.getElementById(
        "notificacoesDropdown"
    );

const notificacaoToast =
    document.getElementById(
        "notificacaoToast"
    );

let idsNotificacoesVistas = [];


// -----------------------------------------
// DROPDOWN DE NOTIFICAÇÕES
// -----------------------------------------

if (dropdownNotificacoes &&
    notificacaoCount) {

    dropdownNotificacoes.addEventListener(
        "shown.bs.dropdown",
        function () {

            const notificacoesNaoLidas =
                dropdownNotificacoes.querySelectorAll(
                    ".notificacao-dropdown-item[data-lida='false']"
                );

            idsNotificacoesVistas =
                Array.from(notificacoesNaoLidas)
                    .map(item =>
                        parseInt(
                            item.dataset.notificacaoId
                        )
                    )
                    .filter(Number.isFinite);
        });


    dropdownNotificacoes.addEventListener(
        "hidden.bs.dropdown",
        async function () {

            if (idsNotificacoesVistas.length === 0) {
                return;
            }

            const idsParaMarcar =
                [...idsNotificacoesVistas];

            idsNotificacoesVistas = [];

            const tokenInput =
                document.querySelector(
                    "#notificacoesAntiforgeryForm " +
                    "input[name='__RequestVerificationToken']"
                );

            if (!tokenInput) {
                console.error(
                    "Token antiforgery não encontrado."
                );

                return;
            }

            const formData =
                new FormData();

            idsParaMarcar.forEach(id => {
                formData.append(
                    "ids",
                    id.toString()
                );
            });

            formData.append(
                "__RequestVerificationToken",
                tokenInput.value
            );

            try {
                const resposta = await fetch(
                    "/Notificacoes?handler=MarcarDropdownLidas",
                    {
                        method: "POST",
                        body: formData,
                        keepalive: true
                    });

                if (!resposta.ok) {
                    throw new Error(
                        `Erro HTTP ${resposta.status}`
                    );
                }

                const resultado =
                    await resposta.json();

                idsParaMarcar.forEach(id => {

                    const item =
                        dropdownNotificacoes.querySelector(
                            `[data-notificacao-id="${id}"]`
                        );

                    if (!item) {
                        return;
                    }

                    item.dataset.lida = "true";

                    item.classList.remove(
                        "notificacao-dropdown-nao-lida"
                    );
                });

                let total =
                    parseInt(
                        notificacaoCount.textContent || "0"
                    );

                total = Math.max(
                    0,
                    total - resultado.marcadas
                );

                notificacaoCount.textContent =
                    total;

                if (total === 0) {
                    notificacaoCount.classList.add(
                        "d-none"
                    );
                }
            }
            catch (erro) {
                console.error(
                    "Erro ao marcar notificações como lidas:",
                    erro
                );
            }
        });
}


// -----------------------------------------
// TOAST
// -----------------------------------------

if (notificacaoToast) {

    notificacaoToast.addEventListener(
        "click",
        function (evento) {

            if (evento.target.closest(
                "[data-bs-dismiss='toast']"
            )) {
                return;
            }

            const link =
                notificacaoToast.dataset.link;

            if (link) {
                window.location.href = link;
            }
        });
}


// -----------------------------------------
// ADICIONAR NOTIFICAÇÃO AO DROPDOWN
// -----------------------------------------

function adicionarNotificacaoDropdown(
    notificacao) {

    const lista =
        document.getElementById(
            "notificacoesDropdownLista"
        );

    if (!lista) {
        return;
    }

    const vazio =
        document.getElementById(
            "notificacoesDropdownVazio"
        );

    if (vazio) {
        vazio.remove();
    }

    const item =
        document.createElement("div");

    item.className =
        "notificacao-dropdown-item " +
        "notificacao-dropdown-nao-lida";

    item.dataset.notificacaoId =
        notificacao.id;

    item.dataset.lida = "false";

    const titulo =
        document.createElement("div");

    titulo.className =
        "fw-semibold";

    titulo.textContent =
        notificacao.titulo;


    const mensagem =
        document.createElement("div");

    mensagem.className =
        "notificacao-dropdown-mensagem";

    mensagem.textContent =
        notificacao.mensagem;


    const data =
        document.createElement("small");

    data.className =
        "text-muted";

    const dataNotificacao =
        new Date(
            notificacao.criadoEm
        );

    data.textContent =
        dataNotificacao.toLocaleString(
            "pt-PT",
            {
                day: "2-digit",
                month: "2-digit",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit"
            });


    item.appendChild(titulo);
    item.appendChild(mensagem);
    item.appendChild(data);

    lista.prepend(item);


    const itens =
        lista.querySelectorAll(
            ".notificacao-dropdown-item"
        );

    if (itens.length > 5) {
        itens[itens.length - 1]
            .remove();
    }
}


// -----------------------------------------
// SIGNALR
// -----------------------------------------

if (notificacaoCount) {

    if (typeof signalR === "undefined") {

        console.error(
            "SignalR não foi carregado."
        );
    }
    else {

        const notificacoesConnection =
            new signalR.HubConnectionBuilder()
                .withUrl(
                    "/hubs/notificacoes"
                )
                .withAutomaticReconnect()
                .build();


        notificacoesConnection.on(
            "ReceberNotificacao",
            function (notificacao) {

                adicionarNotificacaoDropdown(
                    notificacao
                );


                let total =
                    parseInt(
                        notificacaoCount.textContent ||
                        "0"
                    );

                total++;

                notificacaoCount.textContent =
                    total;

                notificacaoCount.classList.remove(
                    "d-none"
                );


                if (notificacaoToast) {

                    notificacaoToast.dataset.link =
                        notificacao.link ?? "";

                    notificacaoToast.classList.toggle(
                        "notificacao-toast-clicavel",
                        Boolean(
                            notificacao.link
                        )
                    );


                    const tituloToast =
                        document.getElementById(
                            "notificacaoToastTitulo"
                        );

                    const mensagemToast =
                        document.getElementById(
                            "notificacaoToastMensagem"
                        );


                    if (tituloToast) {
                        tituloToast.textContent =
                            notificacao.titulo;
                    }

                    if (mensagemToast) {
                        mensagemToast.textContent =
                            notificacao.mensagem;
                    }


                    const toast =
                        bootstrap.Toast
                            .getOrCreateInstance(
                                notificacaoToast,
                                {
                                    delay: 5000
                                }
                            );

                    toast.show();
                }
            });


        notificacoesConnection.start()
            .then(function () {

                console.log(
                    "SignalR de notificações ligado."
                );
            })
            .catch(function (erro) {

                console.error(
                    "Erro ao ligar às notificações:",
                    erro
                );
            });
    }
}