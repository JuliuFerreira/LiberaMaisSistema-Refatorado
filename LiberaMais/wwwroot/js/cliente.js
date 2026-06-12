
document.addEventListener('DOMContentLoaded', function () {

    // Máscara CPF
    const cpf = document.getElementById('cpf');

    if (cpf) {

        cpf.addEventListener('input', function (e) {

            let value = e.target.value.replace(/\D/g, '');

            value = value.replace(/^(\d{3})(\d)/, '$1.$2');
            value = value.replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3');
            value = value.replace(/\.(\d{3})(\d)/, '.$1-$2');

            e.target.value = value;

        });

    }

    // Máscara telefone
    const telefone = document.getElementById('telefone');

    if (telefone) {

        telefone.addEventListener('input', function (e) {

            let value = e.target.value.replace(/\D/g, '');

            value = value.replace(/^(\d{2})(\d)/g, '($1) $2');
            value = value.replace(/(\d{5})(\d)/, '$1-$2');

            e.target.value = value;

        });

    }

});

const cep = document.getElementById('cep');

if (cep) {

    cep.addEventListener('blur', function () {

        let valorCep = this.value.replace(/\D/g, '');

        if (valorCep.length !== 8)
            return;

        fetch(`https://viacep.com.br/ws/${valorCep}/json/`)
            .then(response => response.json())
            .then(data => {

                if (data.erro)
                    return;

                document.getElementById('rua').value = data.logradouro;
                document.getElementById('bairro').value = data.bairro;
                document.getElementById('cidade').value = data.localidade;
                document.getElementById('estado').value = data.uf;

            });

    });

}

