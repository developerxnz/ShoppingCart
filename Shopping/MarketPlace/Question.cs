namespace Shopping.MarketPlace;

public abstract class Question
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public string Event { get; set; }
    
    public string CustomerAttribute { get; set; }
    
    public abstract QuestionType QuestionType { get; }
    
}