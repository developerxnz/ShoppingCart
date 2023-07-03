namespace Shopping.MarketPlace;

public enum QuestionType
{
    MultiChoice,
    MultiSelect,
    Text
}

public record Option(string Key, string Value);

public abstract class Question
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public string? Event { get; set; }
    
    public string? CustomerAttribute { get; set; }
    
    public abstract QuestionType QuestionType { get; }
}

public sealed class Text : Question, ISlide
{
    public override QuestionType QuestionType => QuestionType.Text;
}

public sealed class MultiChoice : Question, ISlide
{
    public override QuestionType QuestionType => QuestionType.MultiChoice;
    
    public List<Option> Options { get; set; }
}

public sealed class MultiSelect : Question, ISlide
{
    public override QuestionType QuestionType => QuestionType.MultiSelect;
    
    public List<Option> Options { get; set; }
}