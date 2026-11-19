using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace ConsoleSelenium.Tests;

[TestClass]
public class BuscaDesafioTest
{
    private const string DuckDuckGoUrl = "https://duckduckgo.com";
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");

        _driver = new ChromeDriver(options);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }

    [TestCleanup]
    public void Teardown()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    private IWebElement LocalizarCampoBusca()
    {
        return _wait.Until(drv =>
        {
            var campo = drv.FindElement(By.Name("q"));
            return campo.Displayed && campo.Enabled ? campo : null;
        })!;
    }

    [TestMethod]
    public void Exercicio1_PesquisaBasica_ValidarTituloOuUrl()
    {
        // 1 e 2. Navegar até https://duckduckgo.com e localizar o campo name="q"
        _driver.Navigate().GoToUrl(DuckDuckGoUrl);
        var campoBusca = LocalizarCampoBusca();

        // 3 e 4. Digitar "Selenium WebDriver C#" e enviar com Keys.Enter
        const string termoBusca = "Selenium WebDriver C#";
        campoBusca.SendKeys(termoBusca + Keys.Enter);

        // 5. Aguardar carregamento e validar que título ou URL contém o termo pesquisado
        _wait.Until(drv =>
            drv.Title.Contains("Selenium", StringComparison.OrdinalIgnoreCase) ||
            Uri.UnescapeDataString(drv.Url).Contains("Selenium", StringComparison.OrdinalIgnoreCase));

        var urlDecodificada = Uri.UnescapeDataString(_driver.Url);
        var contemTermo =
            _driver.Title.Contains("Selenium", StringComparison.OrdinalIgnoreCase) ||
            urlDecodificada.Contains("Selenium", StringComparison.OrdinalIgnoreCase);

        Assert.IsTrue(
            contemTermo,
            $"Esperava encontrar o termo pesquisado no título ('{_driver.Title}') ou na URL ('{urlDecodificada}').");
    }

    [TestMethod]
    public void Exercicio2_Autocompletar_ClicarTerceiraSugestao()
    {
        // 1 e 2. Acessar DuckDuckGo e digitar "C# unit testing" sem pressionar Enter
        _driver.Navigate().GoToUrl(DuckDuckGoUrl);
        var campoBusca = LocalizarCampoBusca();
        campoBusca.SendKeys("C# unit testing");

        // 3. Aguardar sugestões dinâmicas no DOM usando WebDriverWait
        By seletorSugestoes = By.CssSelector(".search-suggestions-list li, .sbox-suggestions li, [role='option']");
        _wait.Until(drv => drv.FindElements(seletorSugestoes).Count(el => el.Displayed) >= 3);

        var sugestoesVisiveis = _driver.FindElements(seletorSugestoes)
            .Where(el => el.Displayed)
            .ToList();

        Assert.IsTrue(
            sugestoesVisiveis.Count >= 3,
            $"Eram esperadas ao menos 3 sugestões visíveis, mas foram encontradas {sugestoesVisiveis.Count}.");

        // 4. Clicar na 3ª opção sugerida (índice 2)
        var terceiraSugestao = sugestoesVisiveis[2];
        var textoSugestao = terceiraSugestao.Text.Trim();
        terceiraSugestao.Click();

        // 5. Validar redirecionamento para a página de resultados da opção escolhida
        _wait.Until(drv =>
            drv.Url.Contains("q=", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(drv.Url, DuckDuckGoUrl + "/", StringComparison.OrdinalIgnoreCase));

        var urlDecodificada = Uri.UnescapeDataString(_driver.Url);
        var tokens = textoSugestao
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 2)
            .ToList();

        var redirecionouCorretamente = tokens.Count > 0
            ? tokens.Any(token =>
                urlDecodificada.Contains(token, StringComparison.OrdinalIgnoreCase) ||
                _driver.Title.Contains(token, StringComparison.OrdinalIgnoreCase))
            : urlDecodificada.Contains("unit", StringComparison.OrdinalIgnoreCase);

        Assert.IsTrue(
            redirecionouCorretamente,
            $"A navegação não refletiu a sugestão '{textoSugestao}'. URL atual: '{urlDecodificada}', Título: '{_driver.Title}'.");
    }

    [TestMethod]
    public void Exercicio3_NavegacaoHistorico_VoltarParaResultados()
    {
        // 1. Realizar busca inicial por "Automated Testing"
        _driver.Navigate().GoToUrl(DuckDuckGoUrl);
        var campoBusca = LocalizarCampoBusca();
        campoBusca.SendKeys("Automated Testing" + Keys.Enter);

        // Aguardar carregamento de um link orgânico externo
        By seletorLinksOrganicos = By.CssSelector("article a[data-testid='result-title-a'], a.result__a, h2 a");
        _wait.Until(drv => drv.FindElements(seletorLinksOrganicos).Any(link =>
        {
            var href = link.GetAttribute("href") ?? string.Empty;
            return link.Displayed &&
                   href.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                   !href.Contains("duckduckgo.com", StringComparison.OrdinalIgnoreCase);
        }));

        var urlResultados = _driver.Url;
        var primeiroLinkOrganico = _driver.FindElements(seletorLinksOrganicos).First(link =>
        {
            var href = link.GetAttribute("href") ?? string.Empty;
            return link.Displayed &&
                   href.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                   !href.Contains("duckduckgo.com", StringComparison.OrdinalIgnoreCase);
        });

        // 2 e 3. Clicar no 1º link orgânico externo e validar mudança de URL
        primeiroLinkOrganico.Click();
        _wait.Until(drv => !string.Equals(drv.Url, urlResultados, StringComparison.OrdinalIgnoreCase));

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(_driver.Url) || string.Equals(_driver.Url, urlResultados, StringComparison.OrdinalIgnoreCase),
            $"A URL não mudou após clicar no resultado orgânico. URL atual: '{_driver.Url}'.");

        // 4. Retornar à página de resultados usando _driver.Navigate().Back()
        _driver.Navigate().Back();

        // 5. Validar que a página de resultados foi restaurada corretamente
        _wait.Until(drv =>
            Uri.UnescapeDataString(drv.Url).Contains("Automated", StringComparison.OrdinalIgnoreCase) ||
            drv.Title.Contains("Automated Testing", StringComparison.OrdinalIgnoreCase));

        var urlRetorno = Uri.UnescapeDataString(_driver.Url);
        var resultadosRestaurados =
            urlRetorno.Contains("Automated", StringComparison.OrdinalIgnoreCase) ||
            _driver.Title.Contains("Automated Testing", StringComparison.OrdinalIgnoreCase);

        Assert.IsTrue(
            resultadosRestaurados,
            $"A página de resultados não foi restaurada corretamente após Navigate().Back(). URL: '{urlRetorno}', Título: '{_driver.Title}'.");
    }
}