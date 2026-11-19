# console-selenium-42-net

Atividade pratica da disciplina de Garantia da Qualidade de Software / Gestao e Qualidade de Software, ministrada pelo Professor Daniel Henrique Matos de Paiva.

O projeto consiste em uma Solution em .NET 10 com aplicacao Console e suite de testes automatizados com MSTest e Selenium WebDriver para cenarios de busca no DuckDuckGo (`https://duckduckgo.com`)[cite: 1, 2].

## Estrutura da Solution

- `ConsoleSelenium42.slnx`: arquivo da Solution .NET 10[cite: 5].
- `ConsoleSelenium/`: projeto principal do tipo Console Application (.NET 10).
- `ConsoleSelenium.Tests/`: projeto de testes unitarios e de automacao utilizando MSTest, `Selenium.WebDriver` e `Selenium.Support`[cite: 1, 2].
- `ConsoleSelenium.Tests/BuscaDesafioTest.cs`: implementacao dos cenarios automatizados no DuckDuckGo[cite: 2].

## Cenarios Automatizados

1. **Pesquisa Basica e Validacao de Titulo/URL**: acessa `https://duckduckgo.com`, localiza o campo de pesquisa (`name="q"`), envia o termo `"Selenium WebDriver C#"` com `Keys.Enter` e valida por `Assert` que o titulo ou a URL da pagina de resultados contem o termo pesquisado[cite: 2].
2. **Interacao com Autocompletar (Suggestions)**: acessa a pagina inicial do DuckDuckGo, digita `"C# unit testing"` sem submeter imediatamente, aguarda as sugestoes dinamicas via `WebDriverWait` (`.search-suggestions-list li`, `.sbox-suggestions li`, `[role='option']`), clica na terceira sugestao e valida o redirecionamento para os resultados correspondentes[cite: 2, 3].
3. **Navegacao por Historico e Botoes de Acao**: realiza uma busca por `"Automated Testing"`, clica no primeiro link organico externo retornado, valida a alteracao da URL, retorna a pagina de resultados com `Navigate().Back()` e valida que os resultados foram restaurados[cite: 3].

## Como Executar

Na raiz do repositorio, execute:

```bash
dotnet test
