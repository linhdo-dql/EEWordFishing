using DataModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class EEWordFishing : GameBase
{
    public List<EERW07ListenAndChooseData> listQuest = new List<EERW07ListenAndChooseData>();
    public GameObject fishLTR;
    public GameObject fishRTL;
    public GameObject pointL;
    public GameObject pointR;
    private static string DATA_PATH = "";
    public SpriteRenderer _bubble;
    private WordFishColor _currentColor;
    //[HideInInspector]
    public float minSpaceBetweenFish = 1.5f;
    public List<GameObject> listLTRFish;
    public List<GameObject> listRTLFish;
    private List<EERW07ListenAndChooseAnswer> _listDisturbanceWords;
    private List<EERW07ListenAndChooseAnswer> _listTrueWords;
    private int _numberFishCaught = 0;
    private int _numberOfFishNeedToCatch = 3;
    public int _currentID = -1;
    

    public override void Start()
    {
        base.Start();
        listLTRFish = new List<GameObject>();
        listRTLFish = new List<GameObject>();  
        _listDisturbanceWords = new List<EERW07ListenAndChooseAnswer>();
        _listTrueWords = new List<EERW07ListenAndChooseAnswer>();
        Vector2 worldBoundary = Camera.main.ScreenToWorldPoint( new Vector2( Screen.width, Screen.height ));
        pointL.transform.position = new Vector3(-worldBoundary.x, 0);
        pointR.transform.position = new Vector3(worldBoundary.x, 0);
        // words = new List<string>() { "hello", "how are you today", "do homework", "work well" };
        // words2 = new List<string>() { "friend", "i am fine", "thanks", "just fun" };
        _currentColor = new WordFishColor(_commonResourceSO.GetColorByKey(ColorKey.PictonBlue).color,
            _commonResourceSO.GetColorByKey(ColorKey.PictonBlue).shadowColor);
        FireBubble();
        PrepareData();
        NextTurn();
        Debug.Log(_wordsForDisturbance.Count);
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextTurn();
        }
    }

    private void CreateALineOfFish(Vector3 startPosition, List<EERW07ListenAndChooseAnswer> datas, int _direction)
    {   
        
       
        startPosition.y = _direction == 1 ? -2f : 2f;
        for (int i = 0; i < datas.Count; i++)
        {
            GameObject fish = Instantiate(_direction == 1 ? fishLTR : fishRTL, startPosition, Quaternion.identity);
            EEWordFish fishWordFish = fish.GetComponent<EEWordFish>();
            fishWordFish.Game = this;
            fishWordFish.Direction = _direction;
            fishWordFish.PopulateData(datas[i].word.text, true);
            float fishLength = fishWordFish.GetFishLength();
            //
            fishWordFish.UpdateFishColor(_currentColor);
            //
            fishWordFish.SetOriginColor(_currentColor);
            if (i + 1 < datas.Count)
            { 
                GameObject nextFishClone = Instantiate(_direction == 1 ? fishLTR : fishRTL, Vector3.zero, Quaternion.identity);
                nextFishClone.GetComponent<EEWordFish>().PopulateData(datas[i+1].word.text, true);
                float nextFishLength = nextFishClone.GetComponent<EEWordFish>().GetFishLength();
                startPosition = _direction == -1 
                ? new Vector3(startPosition.x+ minSpaceBetweenFish + fishLength/2 + nextFishLength/2, startPosition.y, startPosition.z)
                : new Vector3(startPosition.x - minSpaceBetweenFish - fishLength/2 - nextFishLength/2, startPosition.y, startPosition.z);
                Destroy(nextFishClone, 0);
            }
            if (_direction == 1)
            {
                listLTRFish.Add(fish); 
            }
            else
            {
                listRTLFish.Add(fish);  
            }
        }
    }
    public void PrepareData()
    {
        _isTestDataEnable = true;
        LoadData("lessons/55vHuJ2XEhkkSbQXUdlSlu5EBrvf2T9E1633345301/index");
        DATA_PATH = Application.persistentDataPath + "/extracted/0/";
        GetListDisturbanceWords();
        GetTrueWords();
        Debug.LogWarning(_listDisturbanceWords.Count);
        listQuest = new List<EERW07ListenAndChooseData>()
        {
            new(new List<EERW07ListenAndChooseAnswer>(_listDisturbanceWords.GetRange(0,_listDisturbanceWords.Count).ToList().Prepend(_listTrueWords[0]))),
            new(new List<EERW07ListenAndChooseAnswer>(_listDisturbanceWords.GetRange(0,_listDisturbanceWords.Count).ToList().Prepend(_listTrueWords[1]))),
            new(new List<EERW07ListenAndChooseAnswer>(_listDisturbanceWords.GetRange(0,_listDisturbanceWords.Count).ToList().Prepend(_listTrueWords[2]))),
        };
      
    }
            
    private void GetListDisturbanceWords()
    {
        List<Word> listTemp = new(_wordsForDisturbance.FindAll(w => w.text.Split(' ').Length < 2));
        int length = listTemp.Count;
        for (int i = 0; i < length; i++)
        {
            int indexWord = Random.Range(0, listTemp.Count);
            _listDisturbanceWords.Add(SetData(listTemp[indexWord], false));
            listTemp.RemoveAt(indexWord);
        }

    }
    public void PlayAudioQuestion(Action? callback = null)
    {
        SoundManager.GetInstance()
            .AddAudioClip(Resources.Load<AudioClip>("AudioQuestions/Tap the fish with the word" + UnityEngine.Random.Range(1, 2).ToString()))
            .AddAudioClip(Resources.Load<AudioClip>(_listTrueWords[_currentID].audio))
            .SetOnComplete(callback)
            .Play();
    }
    private void GetTrueWords()
    {
        List<Word> listTemp = new(_wordsForTeach.FindAll(w => w.text.Split(' ').Length < 2));
        for (int i = 0; i < 3; i++)
        {
            int indexWord = Random.Range(0, listTemp.Count);
            _listTrueWords.Add(SetData(listTemp[indexWord], true));
            listTemp.RemoveAt(indexWord);
        }
    }

    private static EERW07ListenAndChooseAnswer SetData(Word w, bool isTrueOrDisturbanceWord)
    {
        EERW07ListenAndChooseAnswer answer = new() {word = w, text = w.text, is_true = isTrueOrDisturbanceWord};
        Audio audioOne = w.audio.Find(audio => audio.voices_id == "60");
        audioOne ??= w.audio[0];
        answer.audio = audioOne.link;
        answer.audio = MKStorageHelper.GetFilePathWithoutExtension(answer.audio);
        answer.audio = "words/" + answer.word.word_id + "/" + answer.audio;
        return answer;
    }
    private void NextTurn()
    {
        _numberFishCaught = 0;
        if (_currentID > -1)
        {
           ClearStage();
           
               listLTRFish.Clear();
               listRTLFish.Clear();  
           
        }
        
        _currentID++;
        PlayAudioQuestion();
        List<EERW07ListenAndChooseAnswer> answers = listQuest[_currentID].answers;
        List<EERW07ListenAndChooseAnswer> answer1 = new() {answers[0], answers[1], answers[2]};
        List<EERW07ListenAndChooseAnswer> answer2 = new() {answers[0], answers[3], answers[4]};
        CreateALineOfFish(Vector3.zero, answer1.Shuffle().ToList(), 1);
        CreateALineOfFish(Vector3.zero, answer2.Shuffle().ToList(), -1);
       
        
    }

    private void ClearFishes()
    {
       
    }
    void FireBubble()
    {
        if (_bubble == null) return;
        Vector2 screenSize = HelperManager.GetScreenSize();

        const int numberBubble = 30;

        for (int i = 0; i < numberBubble; i++)
        {
            var b = CreateBubble();

            if (b == null) return;

            float timeBubbleFlow = UnityEngine.Random.Range(5f, 25f);
            LeanTween.moveLocalY(b.gameObject, screenSize.y, timeBubbleFlow).setRepeat(Int32.MaxValue).setOnCompleteOnRepeat(true).setOnComplete(() =>
            {
                timeBubbleFlow = UnityEngine.Random.Range(5f, 25f);
            }).setDelay(i);
        }
    }

    private SpriteRenderer CreateBubble()
    {
        if (_bubble == null) return null;
        var screenSize = HelperManager.GetScreenSize();
        float minX = -screenSize.x / 2;
        float maxX = screenSize.x / 2;
        float y = -screenSize.y / 2 - _bubble.bounds.size.y;
        var bubbleOriginPos = new Vector2(UnityEngine.Random.Range(minX, maxX), y);
        var b = Instantiate(_bubble, _bubble.transform.parent);
        b.transform.position = bubbleOriginPos;
        var scale = UnityEngine.Random.Range(0.2f, 1f);
        b.transform.localScale = new Vector3(scale, scale, scale);
        return b;
    }
    
    public string GetCorrectWord()
    {
        return _listTrueWords[_currentID].text;
    }

    public void OnWrong()
    {
        return;
    }

    public void OnCorrect()
    {  
        _numberFishCaught++;
        if (_numberFishCaught == _numberOfFishNeedToCatch)
        {
            NextTurn();
        }
    }
    public void ClearStage()
    {
        List<float> listTimeToGo = new List<float>();
        var fadeTime = 1.0f;
        listLTRFish.ForEach(f =>
        {
            f.GetComponent<EEWordFish>().FadeOutGameObject();
           
        }); 
        listRTLFish.ForEach(f =>
        {
            f.GetComponent<EEWordFish>().FadeOutGameObject();
           
        });
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("fish");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        SoundManager.GetInstance().AddAudioClip(_audioCommonConfigSO.Correct).Play();
    }

  
}
