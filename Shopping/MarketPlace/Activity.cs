namespace Shopping.MarketPlace;

/*
 * 
 * Assessments
 * One or more Slides
 * Can be completed multiple times
 * Can Calculate an Estimate e.g. Novated Car Lease, Health Insurance
 * Completion Page
 * 
 * Engagements
 * One or more Slides, can be completed multiple times
 * Completion Page
 * 
 * Task
 * One ore more Slides
 * Has points associated
 * 
 * Action
 * Single Slide
 * 
 */

public interface ISlide { }

public interface ISingleSlide { }

public abstract class Slide
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public string NextSlideId { get; set; }
}

public class Single : Slide
{
    
}

public class VideoSlide : Single, ISingleSlide
{
    public Uri Url { get; }
}

public class ExternalLink : Single, ISingleSlide
{
    public Uri Url { get; }
}

public class InternalLink : Single, ISingleSlide
{
    public Uri Url { get; }
}

public abstract class Activity
{
    public List<ISlide> Slides { get; set; }
}

public class Estimate
{
    public int SavingsInCents { get; set; }
}

public class Assessment : Activity
{
    public Estimate Estimate { get; set; }
}

public class Engagement : Activity { }

public class Task : Activity
{
    public int Points => 10;
}

public class Action
{
    public Slide Slide { get; }
}

public class Bundle
{
    public Assessment Assessment { get; }
    public IEnumerable<Action> Actions { get; }
    
    public Bundle()
    {
        Assessment = new Assessment();
        Actions = new List<Action>();
        
        var task = new Task
        {
            Slides = new List<ISlide>()
            {
                new MultiChoice(),
                new MultiSelect(),
                new Text()
            }
        };
        
        foreach (var slide in task.Slides)
        {
            switch (slide)
            {
                case MultiChoice m: Console.WriteLine("m");
                    continue;
                case MultiSelect s: Console.WriteLine("s");
                    continue;
                case Text t: Console.WriteLine("t");
                    continue;
            };
        }
    }

    /*
     * Customer
     *      Container: Customer
     *      Partition  Key: Customer Id
     * 
     *      Id: ActivityId
     *      Type: Activity
     *
     *      Id: Root
     *      Type: Profile
     *
     *      Id: guid
     *      Type: Event
     * 
     * Points
     *      Container: Points
     *      Partition Key: Customer Id
     *      Id: ActivityId
     *      - Only one event per ActivityId 
     * 
     * Customer Activity
     * 
     * Progress Event
     *
     * Activity
     *      Container Activities
     *      Partition Key: Activity Id
     *      Read Model
     *          Id: ReadModel
     *      Aggregate
     *          Id: Root
     * 
     * Activity Read Model
     *      Used
     *          Via Product
     *          Via Task
     *      Updated
     *          When Activity Updated
     *          When Question updated
     * 
     *      How
     *          Get Activity
     *          Events
     *              Activity Updated
     *          Get Question
     *          Events
     *              Question Updated
     *          
     * Product
     *      Partition Key: Product Id
     *      Id: Root
     * 
     * Product Read Model including Activity Summary
     *      Used
     *          Via Category
     *      Updated
     *          When Product Updated
     *              - create
     *              - update
     *              - disabled
     *              - enabled
     *          When Activity Updated
     *              - create
     *              - update
     *              - disabled
     *              - enabled
     *          How
     *              Get Activity Read Model ?
     *
     * Category
     *      Partition Key: Product Id
     *      Id: Root
     * 
     * Category Read Model
     *      Used
     *          Via Dashboard
     *      Updated
     *          When Category
     *              - create
     *              - update
     *              - disabled
     *          When Product
     *              - create
     *              - update
     *              - disabled
     *
     * Tasks - Specific Activity Read Models
     *      Used
     *          Via Activity
     *      Updated
     *          When Activity
     *              - create
     *              - update
     *              - disabled
     *              - Points set to 0
     * 
     * Get Product
     * - Load Product
     * - Load Activities
     *      - Load Questions
     * - Load Customer Records
     *   - By Customer Id,
     *   - By Profile Id
     *   - By Activity Id
     */
    
    /*
     * Created via progress event
     * Partition Key
     * Customer Id
     * Profile Id
     * Activity Id
     * 
     */
    
    public class StepAnswer
    {
        public int StepId { get; }
        
        public string Answer { get; }
        
        public string DataType { get; }
        
        public DateTime CreatedOn { get; }
        
        public DateTime ModifiedOn { get; }
        
    }

    public class AssessmentEntry
    {
        public int CustomerId { get; set; }
        
        public string ProfileId { get; set; }
        
        public int ActivityId { get; set; }
        
        public DateTime CreatedOn { get; }
        
        public DateTime ModifiedOn { get; }
        
        public List<StepAnswer> Steps { get; set; }
    }
}