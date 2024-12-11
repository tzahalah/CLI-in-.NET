using System.CommandLine;


var rootCommand = new RootCommand("root command");
var bundleCommand = new Command("bundle", "bundle code files to single file");
rootCommand.AddCommand(bundleCommand);

var languageOption = new Option<List<string>>(aliases: new[] { "--language", "-lang" }, "Code languages for the bundle")
{
    IsRequired = true,
    Arity = ArgumentArity.OneOrMore,
    AllowMultipleArgumentsPerToken = true
};
languageOption.AddValidator(result =>
{
    var allowedValues = new[] { "all", "c#", "java", "python", "css", "javaScript", "typeScript", "html", "sql" };
    var invalidValues = result.Tokens
        .Select(t => t.Value)
        .Where(v => !allowedValues.Contains(v))
        .ToList();

    if (invalidValues.Any())
    {
        result.ErrorMessage = $"Invalid languages: {string.Join(", ", invalidValues)}. Allowed values are: {string.Join(", ", allowedValues)}";
    }
});
var outputOption = new Option<FileInfo>(new[] { "--output", "-o" }, "file location- path and name");
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Should  indicate the source and location of the code ?");
noteOption.AddValidator(result =>
{
    var val = result.Tokens.First().Value ?? " ";
    if (val != null && !(val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase)))
        result.ErrorMessage = $"Invalid value. Allowed values are:true or false ";
});

var sortOption = new Option<string>(new[] { "--sort", "-s" }, getDefaultValue: () => "alphabet", description: "sort files");
sortOption.AddValidator(result =>
{
    var val = result.Tokens.FirstOrDefault()?.Value;
    if (val != null && !(val.Equals("alphabet") || val.Equals("type")))
        result.ErrorMessage = $"Invalid value. Allowed values are:type or alphabet ";
});
var cleanOption = new Option<bool>(new[] { "--remove-empty-lines", "-rel" });
cleanOption.AddValidator(result =>
{
    var val = result.Tokens.First().Value ?? " ";
    if (val != null && !(val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase)))
        result.ErrorMessage = $"Invalid value. Allowed values are:true or false ";
});

var authorOption = new Option<string>(new[] { "--author", "-a" }) { IsRequired = false };

List<String> language = new List<string>() { "c#", "java", "python", "css", "javaScript", "typeScript", "html", "sql" };
var extentionTolang = new Dictionary<string, string>()
{
    { ".py", "python" },
    {".java", "java" },
    {".cs", "c#" },
    {".css", "css" },
    {".js",  "javaScript"},
    { ".ts","typeScript" },
    {".html", "html" },
    {".sql", "sql" }
};



bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(cleanOption);
bundleCommand.AddOption(authorOption);
bundleCommand.SetHandler((output, lang, isNote, sort, clean, author) =>
{
    List<string> FilesList = new List<string>();
    string p = output.FullName;
    string[] sortedFiles;
    try
    {
        File.Create(p).Dispose(); ;
    }
    catch (Exception e)
    {
        Console.WriteLine("the path is invalid");
    }

    File.WriteAllText(p, $"{author} \n");
    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
    if ((sort.Equals("type")))
        sortedFiles = files.OrderBy(f => Path.GetExtension(f)).ThenBy(f => Path.GetFileName(f)).ToArray();
    else sortedFiles = files.OrderBy(f => Path.GetFileName(f)).ToArray();
    foreach (string file in sortedFiles)
    {
        var l = extentionTolang.GetValueOrDefault(Path.GetExtension(file));
        if (lang[0] == "all" && extentionTolang.ContainsValue(l ?? "") || l != null && lang.Contains(l))
        {
            if (isNote)
                File.AppendAllText(p, Path.GetFileName(file) + "\n");
            if (clean)
            {
                var lines = File.ReadAllLines(file);
                var nonEmptyLines = lines.Where(l => !string.IsNullOrWhiteSpace(l));
                File.AppendAllLines(p, nonEmptyLines);
            }
            else
                File.AppendAllText(p, File.ReadAllText(file));
        }
        File.AppendAllText(p, "\n\n");
    }

    Console.WriteLine("bundle");
}, outputOption, languageOption, noteOption, sortOption, cleanOption, authorOption);




var rspCommand = new Command("create-rsp");
rootCommand.AddCommand(rspCommand);
rspCommand.SetHandler(() =>
{
    String path = "File.rsp";
    File.Create(path).Dispose();
    String author, fileName, langs, note, rel, sort;

    List<String> l = new List<string>();

    Console.WriteLine("Enter the name of new file");
    fileName = Console.ReadLine() ?? " ResponseFile";

    Console.WriteLine("Enter the progarm languages");
    langs = Console.ReadLine();
    if (langs != "all")
    {
        l = langs.Split(" ").ToList();
        langs = isValidList(language, l);
    }

    Console.WriteLine("Do you want note the code source?");
    note = Console.ReadLine();
    if (!string.IsNullOrEmpty(note))
        note = isValid(new List<string> { "true", "false" }, note);
    else note = "true";

    Console.WriteLine("Enter sort type, sort by:\r\ntype\r\nalphabet ");
    sort = Console.ReadLine();
    if (!string.IsNullOrEmpty(sort))
        sort = isValid(new List<string> { "alphabet", "type" }, sort);
    else sort = "alphabet";

    Console.WriteLine("Remove empty rows?");
    rel = Console.ReadLine();
    if (!string.IsNullOrEmpty(rel))
        rel = isValid(new List<string> { "true", "false" }, rel);
    else rel = "true";

    Console.WriteLine("Enter the name of author");
    author = Console.ReadLine() ?? " ";


    File.AppendAllText(path,
        $"package bundle --output {fileName} \n" +
        $"--language {langs} \n" +
        $"--note {note}\n " +
        $"--sort {sort}\n " +
        $"--remove-empty-lines {rel} \n");
    if (!string.IsNullOrEmpty(author))
        File.AppendAllText(path, $"--author {author}");

});

static String isValid(List<String> validVal, String inputVal)
{
    bool valid = validVal.Contains(inputVal);
    while (!valid)
    {
        Console.WriteLine($"invalid input\n the valid inputs are: {String.Join(" ", validVal)} \n try again");
        inputVal = Console.ReadLine();
        valid = string.IsNullOrEmpty(inputVal) || validVal.Contains(inputVal);
    }
    return inputVal ?? "";
}

static String isValidList(List<String> validVal, List<String> inputVal)
{

    string st = String.Join(" ", inputVal);
    Console.WriteLine(st);
    bool valid = inputVal.All(item => validVal.Contains(item));
    while (!valid || inputVal == null)
    {
        Console.WriteLine($"invalid input\n the valid inputs are: {String.Join(" ", validVal)} \n try again");
        st = Console.ReadLine() ?? " ";
        if (st != " ")
        {
            inputVal = st.Split(" ").ToList();
            valid = inputVal.All(item => validVal.Contains(item));
        }
    }

    return st;
}

rootCommand.InvokeAsync(args);
