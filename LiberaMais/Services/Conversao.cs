using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Globalization;
using System.Threading.Tasks;

public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value)) return Task.CompletedTask;

        // Remove pontos de milhar e troca a vírgula por ponto para o C# salvar certo
        value = value.Replace(".", "").Replace(",", ".");

        if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Valor inválido.");
        }

        return Task.CompletedTask;
    }
}