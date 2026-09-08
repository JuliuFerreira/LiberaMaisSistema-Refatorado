
window.addEventListener('load', function () {

            setTimeout(function () {

                var alertas = [
                    document.getElementById("alertaSucesso"),
                    document.getElementById("alertaErro")
                ];

                alertas.forEach(function (alerta) {

                    if (alerta) {

                        alerta.style.transition = "opacity 0.5s ease";
                        alerta.style.opacity = "0";

                        setTimeout(function () {

                            alerta.style.display = "none";

                        }, 500);

                    }

                });

            }, 3000);

        });


document.getElementById("btnBuscarCliente").addEventListener("click", function () {

    var termo = document.getElementById("termoCliente").value;

    if (termo.trim() === "") {
        alert("Digite o nome ou CPF do cliente.");
        return;
    }

    fetch('/Agendamento/BuscarCliente?termo=' + encodeURIComponent(termo))

        .then(response => response.json())

        .then(clientes => {

            var resultado = document.getElementById("resultadoClientes");

            resultado.innerHTML = "";

            if (clientes.length === 0) {

                resultado.innerHTML =
                    '<div class="alert alert-warning mt-2">' +
                    'Nenhum cliente encontrado.' +
                    '</div>';

                return;
            }

            clientes.forEach(cliente => {

                var item = document.createElement("button");

                item.type = "button";

                item.className =
                    "list-group-item list-group-item-action";

                item.innerHTML =
                    '<div class="fw-bold">' +
                    cliente.nome +
                    '</div>' +

                    '<small class="text-muted">' +
                    'CPF: ' + cliente.cpf +
                    ' | Telefone: ' + cliente.fone +
                    '</small>';

                item.addEventListener("click", function () {

                    document.getElementById("ClienteId").value =
                        cliente.id;

                    document.getElementById("ClienteNome").value =
                        cliente.nome;

                    document.getElementById("ClienteCpf").value =
                        cliente.cpf;

                    document.getElementById("ClienteFone").value =
                        cliente.fone;

                    document.getElementById("termoCliente").value =
                        cliente.nome;

                    resultado.innerHTML = "";

                });

                resultado.appendChild(item);

            });

        })

        .catch(error => {

            console.error(error);

            alert("Erro ao buscar clientes.");

        });

});

