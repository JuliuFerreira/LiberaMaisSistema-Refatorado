
        window.addEventListener('load', function () {

            setTimeout(function () {

                var alertas = [
                    document.getElementById("alertaSucesso"),
                    document.getElementById("alertaErro")
                ];

                alertas.forEach(function (alerta) {

                    if (alerta) {

                        alerta.style.transition =
                            "opacity 0.5s ease";

                        alerta.style.opacity = "0";

                        setTimeout(function () {

                            alerta.style.display = "none";

                        }, 500);

                    }

                });

            }, 3000);

        });

