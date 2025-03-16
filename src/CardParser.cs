using System.Text;

namespace scraping_anki;

public static class CardParser
{
    public static List<(string front, string back)> ParseEnglishFile(string? filePath)
    {
        var cards = new List<(string front, string back)>();
        if (filePath != null)
        {
            var lines = File.ReadAllLines(filePath);
            var frontContent = new StringBuilder();
            var backContent = new StringBuilder();
            var collectingFront = false;
            var collectingBack = false;
            foreach (var t in lines)
            {
                var line = t.Trim();
                switch (line)
                {
                    case "FRONT:":
                    {
                        if (collectingBack && frontContent.Length > 0 && backContent.Length > 0)
                        {
                            cards.Add((frontContent.ToString().Trim(), backContent.ToString().Trim()));
                            frontContent.Clear();
                            backContent.Clear();
                        }
                        collectingFront = true;
                        collectingBack = false;
                        break;
                    }
                    case "BACK:":
                        collectingFront = false;
                        collectingBack = true;
                        break;
                    default:
                    {
                        if (collectingFront)
                        {
                            frontContent.AppendLine(line);
                        }
                        else if (collectingBack)
                        {
                            backContent.AppendLine(line);
                        }

                        break;
                    }
                }
            }
            if (frontContent.Length > 0 && backContent.Length > 0)
            {
                cards.Add((frontContent.ToString().Trim(), backContent.ToString().Trim()));
            }
        }

        Logger.LogToFile($"Loaded {cards.Count} cards from {filePath}");
        return cards;
    }
}