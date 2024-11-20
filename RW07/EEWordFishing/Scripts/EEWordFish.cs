#nullable enable
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EEWordFish : MonoBehaviour, IPointerClickHandler
{
    public enum FishState
    {
        NORMAL,
        WRONG,
        CORRECT
    }
    private float _fishHeadLength = 2.7f;
    private int _direction = 1;
    private List<string> _wordAABBs;
    private EEWordFishing game;
    private WordFishColor _color;
    private WordFishColor _originColor;
    private readonly char[] _shortLetter = {'t','i','l',' '};
    private readonly char[] _longLetter = {'w','m'};
    private SkeletonAnimation _eyeAnimation;
    private SkeletonAnimation _finTailAnimation;
    private SkeletonAnimation _finBody1Animation;
    private SkeletonAnimation _finBody2Animation;
    [SerializeField] private GameObject _fishBody;
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private float _swimSpeed; //unit per second
    [SerializeField] private CapsuleCollider2D _collider;
    [SerializeField] private CommonResourceSO _commonResourceSo;
    private bool _enableCollider = true;

    public bool EnableCollider
    {
        get => _enableCollider;
        set => _enableCollider = value;
    }
    public GameObject fishHead;
    public GameObject fishTail;
    public GameObject fishFinTail;
    public GameObject fishTopFin;
    public GameObject fishFinBody1;
    public GameObject fishFinBody2;
    public GameObject eye;
    public GameObject fishHeadSup;
    public GameObject fishMount;
    public EEWordFishing Game
    {
        get => game;
        set => game = value;
    }
    public int Direction
    {
        get => _direction;
        set => _direction = value;
    }
    private readonly bool _shouldSwim;
    public EEWordFish()
    {
        _shouldSwim = true;
    }
    void Start()
    {
        _collider = GetComponent<CapsuleCollider2D>();
        _eyeAnimation = eye.GetComponent<SkeletonAnimation>();
        _finTailAnimation = fishFinTail.GetComponent<SkeletonAnimation>();
        _finBody1Animation = fishFinBody1.GetComponent<SkeletonAnimation>();
        _finBody2Animation = fishFinBody2.GetComponent<SkeletonAnimation>();
        _wordAABBs = new List<string>();
    }

    void Update()
    {
        if (_shouldSwim) Swim();
    }

    private void Swim()
    {
        transform.Translate(Time.deltaTime * _swimSpeed * _direction * Vector3.right);
        _collider.enabled = EnableCollider;
    }

    private float GetScaleFactorBaseOnText(string text)
    {
        text = text.ToLower();
        float scaleFactor = 0;
        for (var i = 0; i < text.Length-1; i++)
        {
            if (_shortLetter.Contains(text[i]))
            {
                scaleFactor += 0.52f;
            } else if (_longLetter.Contains(text[i]))
            {
                scaleFactor += 0.67f;
            } else
            {
                scaleFactor += 0.62f;
            }
        }
        return scaleFactor;
    }
    
    private void UpdateFishProperties()
    {   
        float scaleFactor = GetScaleFactorBaseOnText(_text.text);
        //ResetBodyFin
        SpriteRenderer fistTopFinStart = fishTopFin.GetComponent<SpriteRenderer>();
        fistTopFinStart.size = new Vector2(1, 0.43f);
        //Body
        Vector3 bodyScale = _fishBody.transform.localScale;
        bodyScale = new Vector3(scaleFactor, bodyScale.y, bodyScale.z);
        _fishBody.transform.localScale = bodyScale;
        //BodyFin
        SpriteRenderer fistTopFinRenderer = fishTopFin.GetComponent<SpriteRenderer>();
        Bounds bounds = fistTopFinRenderer.bounds;
        fistTopFinRenderer.size = new Vector2(bounds.size.x + bounds.size.x * scaleFactor - 0.75f, bounds.size.y);
        //Head
        Vector3 headPos = fishHead.transform.localPosition;
        float wHead = fishHead.GetComponent<SpriteRenderer>().bounds.size.x;
        headPos = new Vector3(scaleFactor*_direction/ 2 + wHead*_direction/2, headPos.y, headPos.z);
        fishHead.transform.localPosition = headPos;
        //Tail
        Vector3 tailPos = fishTail.transform.localPosition;
        float wTail = fishTail.GetComponent<SpriteRenderer>().bounds.size.x;
        tailPos = new Vector3(-scaleFactor*_direction / 2 - wTail*_direction/2, tailPos.y, tailPos.z);
        fishTail.transform.localPosition = tailPos;
        //Text
        var textLocalPosition = _text.gameObject.transform.localPosition;
        _text.gameObject.transform.localPosition = new Vector3(headPos.x - (_direction == 1 ? 0 : _direction)*_fishHeadLength , textLocalPosition.y, 
        textLocalPosition.z);
        //
        
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        game.OnInteract();
        if (game.GetCorrectWord() == _text.text)
        {
            OnCorrect();
        }
        else
        {
            OnWrong();
        }
    }

    private void OnWrong()
    {
        DoEffect(false);
        game.OnWrong();
    }

    private void OnCorrect()
    {
        DoEffect(true);
        game.OnCorrect();
    }

    private void ResetState()
    {
        ChangeFishState(FishState.NORMAL);
        ResetText();
        ResetAnimation();
        PopulateData(_text.text, true);
    }

    private void ResetAnimation()
    {
        _finBody1Animation.Skeleton.A = 1;
        _finBody2Animation.Skeleton.A = 1;
        _finTailAnimation.Skeleton.A = 1;
        _eyeAnimation.Skeleton.A = 1;
    }
    private void ResetText()
    {   
        List<EERW07ListenAndChooseAnswer> listCurrentAnswer = game.listQuest[game._currentID].answers;
        if (_wordAABBs.Count == 2)
        {
            if (!_wordAABBs.Contains(listCurrentAnswer[0].text))
            {
                _text.text = listCurrentAnswer[0].text;
                
            }
            _wordAABBs.Clear();
            
        }
        else
        {
            _text.text = listCurrentAnswer[Random.Range(0, listCurrentAnswer.Count - 1)].text;
        }
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1);
      
    }

    private async Task DoEffect(bool isCorrect)
    {
        List<EERW07ListenAndChooseAnswer> currentAnswer = game.listQuest[game._currentID].answers;
        EERW07ListenAndChooseAnswer currentWord = currentAnswer.Find(w => w.text == _text.text);
        if (isCorrect)
        {   
            ChangeFishState(FishState.CORRECT);
            SoundManager.GetInstance().PlayAudioFromNewSource(Resources.Load<AudioClip>(currentWord.audio)) ;
            HelperManager.DoSpreadEffect(transform, HelperManager.Environment.OBJECT, true, Vector2.zero, 4f, 10, null, 0.8f, 1, false);
            await Task.Delay(1000);
            FadeOutGameObject();
           
            return;
        }

        SoundManager.GetInstance().PlayAudioFromNewSource(Resources.Load<AudioClip>("AudioQuestions/Wrong"));
        ChangeFishState(FishState.WRONG);
        HelperManager.DoSpreadEffect(transform, HelperManager.Environment.OBJECT, false, Vector2.zero, 4f, 10, null, 0.8f, 1, false);
    }

    public void FadeOutGameObject()
    {
        StartCoroutine(FadeInAndOut(fishTopFin, false, 0.2f));
        StartCoroutine(FadeInAndOut(fishHead, false, 0.2f));
        StartCoroutine(FadeInAndOut(fishTail, false, 0.2f));
        StartCoroutine(FadeInAndOut(_fishBody, false, 0.2f));
        StartCoroutine(FadeInAndOut(_text.gameObject, false, 0.2f));
        StartCoroutine(FadeInAndOut(fishFinBody1, false, 0.2f));
        StartCoroutine(FadeInAndOut(fishFinBody2, false, 0.2f));
        StartCoroutine(FadeInAndOut(fishFinTail, false, 0.2f));
        StartCoroutine(FadeInAndOut(eye, false, 0.2f));
        StartCoroutine(FadeInAndOut(fishHeadSup, false, 0.2f));
        StartCoroutine(FadeInAndOut(fishMount, false, 0.2f));
    }

    public void UpdateFishColor(WordFishColor fishColor)
    {
        if (_fishBody == null) return;
        _color = fishColor;
        var bodySprite = _fishBody.GetComponent<SpriteRenderer>();
        var headSprite = fishHead.GetComponent<SpriteRenderer>();
        var tailSprite = fishTail.GetComponent<SpriteRenderer>();
        var topFin = fishTopFin.GetComponent<SpriteRenderer>();
        var fishMountS = fishMount.GetComponent<SpriteRenderer>();
        var fishHeadSupS = fishHeadSup.GetComponent<SpriteRenderer>();
        if (bodySprite != null) bodySprite.color = _color.bodyColor;
        if (headSprite != null) headSprite.color = _color.bodyColor;
        if (tailSprite != null) tailSprite.color = _color.bodyColor;
        if (topFin != null) topFin.color = _color.finColor;
        if (fishMountS != null) fishMountS.color = _color.finColor;
        if (fishHeadSupS != null) fishHeadSupS.color = _color.finColor;
    }
    
    public void ResetFishPosition()
    {
        _wordAABBs.Add(_text.text);
        ResetState();
        float fishLength = GetFishLength(); // todo: 
        if (_direction == 1)
        {
            transform.position = new Vector3(game.listLTRFish.Last().transform.position.x - game.listLTRFish.Last().GetComponent<EEWordFish>().GetFishLength()/2 - fishLength/2 - game.minSpaceBetweenFish, -2f);
            game.listLTRFish = game.listLTRFish.Skip(1).Append(gameObject).ToList();
            
        }
        else
        {   
            transform.position =new Vector3(game.listRTLFish.Last().transform.position.x + game.listRTLFish.Last().GetComponent<EEWordFish>().GetFishLength()/2 + fishLength/2 + game.minSpaceBetweenFish, 2f);
            game.listRTLFish = game.listRTLFish.Skip(1).Append(gameObject).ToList();
        }
    }

    public float GetFishLength()
    {
        float wHead = fishHead.GetComponent<SpriteRenderer>().bounds.size.x;
        float wBody = _fishBody.GetComponent<SpriteRenderer>().bounds.size.x;
        float wTail = fishTail.GetComponent<SpriteRenderer>().bounds.size.x;
        float wFin = fishFinTail.GetComponent<MeshRenderer>().bounds.size.x;
        float wMount = fishMount.GetComponent<SpriteRenderer>().bounds.size.x;
        float fishLength = wMount + wHead + wBody + wTail + wFin;
        _collider.size = new Vector2(fishLength, 3f);
        return fishLength;
    } 
  
    public void PopulateData(string text, bool isUpdateProperties =  false)
    {
        _text.text = text;
        if (isUpdateProperties)
        {
            UpdateFishProperties();
        }
    }
    
    public void ChangeFishState(FishState state)
    {
    
        if (eye == null) return;
        ColorKey colorKey = ColorKey.White;
        
        switch (state)
        {
            case FishState.NORMAL:
                {
                
                    colorKey = ColorKey.White;
                    EnableCollider = true;
                    _eyeAnimation.Initialize(true);
                    _eyeAnimation.skeleton.SetSkin("mj-fish-blue");
                    _finTailAnimation.skeleton.SetSkin("mj-fish-blue");
                    _finBody1Animation.skeleton.SetSkin("mj-fish-blue");
                    _finBody2Animation.skeleton.SetSkin("mj-fish-blue");
                    break;
                }
    
    
            case FishState.WRONG:
                {
                    colorKey = ColorKey.Red;
                    EnableCollider = false;
                    _eyeAnimation.skeleton.SetSkin("mj-fish-red");
                    _finTailAnimation.skeleton.SetSkin("mj-fish-red");
                    _finBody1Animation.skeleton.SetSkin("mj-fish-red");
                    _finBody2Animation.skeleton.SetSkin("mj-fish-red");
                    _eyeAnimation.ClearState();
                    break;
                }
    
            case FishState.CORRECT:
                {
                    colorKey = ColorKey.Green;
                    EnableCollider = false;
                    _eyeAnimation.skeleton.SetSkin("mj-fish-green");
                    _finTailAnimation.skeleton.SetSkin("mj-fish-green");
                    _finBody1Animation.skeleton.SetSkin("mj-fish-green");
                    _finBody2Animation.skeleton.SetSkin("mj-fish-green");
                    _eyeAnimation.ClearState();
                    break;
                }
    
        }
    
        WordFishColor fishColor;
    
        if (colorKey != ColorKey.White)
        {
            var goColor = _commonResourceSo?.GetColorByKey(colorKey);
            fishColor = new WordFishColor(goColor!.color, goColor.shadowColor);
        }
        else
        {
            fishColor = new WordFishColor(_originColor!);
        }
        UpdateFishColor(fishColor);
        
    }
    
   

    public void SetOriginColor(WordFishColor currentColor)
    {
        _originColor = currentColor;
    }
    IEnumerator FadeInAndOut(GameObject objectToFade, bool fadeIn, float duration) {
        float counter = 0f;

        //Set Values depending on if fadeIn or fadeOut
        float a, b;
        if (fadeIn)
        {
            a = 0;
            b = 1;
        }
        else
        {
            a = 1;
            b = 0;
        }

        int mode = 0;
        Color currentColor = Color.clear;
      
        SpriteRenderer tempSPRenderer = objectToFade.GetComponent<SpriteRenderer>();
        Image tempImage = objectToFade.GetComponent<Image>();
        RawImage tempRawImage = objectToFade.GetComponent<RawImage>();
        MeshRenderer tempRenderer = objectToFade.GetComponent<MeshRenderer>();
        TextMeshPro tempText = objectToFade.GetComponent<TextMeshPro>();
        SkeletonAnimation skeletonAnimation = objectToFade.GetComponent<SkeletonAnimation>();
        //Check if this is a Sprite
        if (tempSPRenderer != null)
        {
            currentColor = tempSPRenderer.color;
            mode = 0;
        }
        //Check if Image
        else if (tempImage != null)
        {
            currentColor = tempImage.color;
            mode = 1;
        }
        //Check if RawImage
        else if (tempRawImage != null)
        {
            currentColor = tempRawImage.color;
            mode = 2;
        }
        //Check if Text 
        else if (tempText != null)
        {
            currentColor = tempText.color;
            mode = 3;
        }
        //Check if SkeletonAnimation 
        else if (skeletonAnimation != null)
        {
            mode = 4;
        }
    
        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(a, b, counter / duration);

            switch (mode)
            {
                case 0:
                    tempSPRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 1:
                    tempImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 2:
                    tempRawImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 3:
                    tempText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 4:
                    skeletonAnimation.Skeleton.A = alpha;
                    break;
            }
            yield return null;
        }
        
        
    }
}
