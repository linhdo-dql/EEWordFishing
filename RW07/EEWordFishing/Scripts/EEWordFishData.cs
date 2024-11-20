using System.Collections.Generic;

public class EERW07ListenAndChooseAnswer
{
    public DataModel.Word word;
    public string audio;
    public string image_path;
    public string text;
    public bool is_true;
};

public class EERW07ListenAndChooseData
{
    public List<EERW07ListenAndChooseAnswer> answers;

    public EERW07ListenAndChooseData(List<EERW07ListenAndChooseAnswer> answers)
    {
        this.answers = answers;
    }
};