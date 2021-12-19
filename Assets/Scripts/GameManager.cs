using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Wywalone wszystko w jednym skrypcie
public class GameManager : MonoBehaviour
{
    public int target {
        get
        {

            return int.Parse(ui.target.text);
        }
        set
        {
            ui.target.text = value.ToString();
        }
    }

    public int score
    {
        get
        {
            return int.Parse(ui.score.text);
        }
        set
        {
            ui.score.fontSize = ui.scoreSize + value * ui.scoreSizeStep;
            ui.score.text = value.ToString();
        }
    }

    public int value
    {
        get
        {
            return int.Parse(ui.value.text);
        }
        set
        {
            ui.value.text = value.ToString();
        }
    }



    [Header("Dices Stuff")]
    public Sprite evilDice;
    List<int> evilDices = new List<int>();
    public Sprite[] notEvilDices;

    public Rigidbody2D[] dices;
    private Vector3[] dicesStartingPosition = new Vector3[6];
    [SerializeField] private AnimationCurve animation;
    public float timePerDice = 0.1f;




    [Header("UI")]
    public UI ui;

    [Header("Audio")]
    public Audio audio;

    private List<int> indexes = new List<int>();

    private int dice;
    private float time;


    private bool wait = false;
    public float timeBetweenRounds = 1;
    private float timerounds = 0;
    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            dicesStartingPosition[i] = dices[i].transform.position;
            dices[i].GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    public AnimationCurve returnAnimation;

    private Vector3[] startlerp = new Vector3[6];
    private Quaternion[] startlerpquaterions = new Quaternion[6];
    
    private void Update()
    {
        ui.space.sprite = ui.spaces[Input.GetKey(KeyCode.Space) ? 1 : 0];

        if (wait)
        {
            timerounds += Time.deltaTime / timeBetweenRounds;

            if (requestspace)
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ui.title.sprite = ui.titles[0];
                    requestspace = false;
                    ui.sparks.SetActive(false);

                    timerounds = timeBetweenRounds / 2;

                    for (int i = 0; i < 6; i++)

                    {
                        startlerp[i] = dices[i].transform.position;
                        startlerpquaterions[i] = dices[i].transform.rotation;
                    }

                }
            if (timerounds < timeBetweenRounds / 2)
            {
                for (int i = 0; i < 6; i++)

                {
                    startlerp[i] = dices[i].transform.position;
                    startlerpquaterions[i] = dices[i].transform.rotation;
                }
            }

            else
            {
                if (requestspace)
                    return;
                for (int i = 0; i < 6; i++)
                {
                    dices[i].transform.position = Vector3.Lerp(startlerp[i], dicesStartingPosition[i], returnAnimation.Evaluate(timerounds / timeBetweenRounds));
                    dices[i].transform.rotation = Quaternion.Lerp(startlerpquaterions[i], Quaternion.identity, returnAnimation.Evaluate(timerounds / timeBetweenRounds));
                }
            }

                       return;
        }

        #region tutaj s¹ koœci robi¹ce brrr, ale nie mogê wpaœæ na nazwê funkcji dla tego... dlatego bêdzie to w regionie
    
        time += Time.deltaTime;

        dices[dice].transform.localPosition = new Vector3(dices[dice].transform.localPosition.x, animation.Evaluate(time / timePerDice * 2), 0);
    
        if (time >= timePerDice)
        {
            if (!indexes.Contains(dice))
                dices[dice].transform.localPosition = new Vector3(dices[dice].transform.localPosition.x, 0, 0);

            time = 0;

            do
            {
                dice++;
                dice %= 6;
            } while (indexes.Contains(dice));
        }
        #endregion


        if (Input.GetKeyDown(KeyCode.Space))
        {
            audio.source.PlayOneShot(audio.a);
            if (!evilDices.Contains(dice) && value + dice + 1 <= target)
            {
                score++;
                if (target == 5)
                    StartCoroutine(MorePoints());
            }

            if (!indexes.Contains(dice))
            {
                if (evilDices.Contains(dice))
                {
                    ui.title.sprite = ui.titles[2];
                    ui.sparks.SetActive(false);

                    requestspace = true;
                    StartCoroutine(ShakeTarget());
                    StartCoroutine(Wait());
                }

                if (dice + 1 != target)
                    value += dice + 1;

                indexes.Add(dice);
                dices[dice].GetComponent<BoxCollider2D>().enabled = true;

                time = timePerDice;
                dices[dice].GetComponent<Rigidbody2D>().AddForce((ui.target.transform.position - dices[dice].transform.position).normalized * 800);
                dices[dice].angularVelocity = 100;


                if (value >= target)
                {
                    requestspace = value != target;
                    if (requestspace)
                    {
                        ui.title.sprite = ui.titles[2];
                        ui.sparks.SetActive(false);
                    }
                    StartCoroutine(Wait());

                    if(value > target)
                        StartCoroutine(ShakeTarget());

                }
            }
        }
    }
    bool requestspace = false;
    IEnumerator ShakeTarget()
    {
        audio.source.PlayOneShot(audio.b);

        float elapsed = 0.0f;
        Quaternion originalRotation = ui.target.transform.parent.rotation;

        while (elapsed < 0.2f)
        {

            elapsed += Time.deltaTime;
            float z = Random.value * 15 - (15 / 2);
            ui.target.transform.parent.eulerAngles = new Vector3(originalRotation.x, originalRotation.y, originalRotation.z + z);
            yield return null;
        }

        ui.target.transform.parent.rotation = originalRotation;
    }

    IEnumerator MorePoints()
    {
        yield return new WaitForSeconds(0.1f);
        score++;
    }

    IEnumerator Wait()
    {
        wait = true;
        timerounds = 0;

        if (!requestspace)
        {
            bool breakout = false;
            for (int i = 0; i < 6; i++)
            {

               
                if (evilDices.Contains(i))
                    continue;
                if (!indexes.Contains(i))
                {

                    StartCoroutine(Plus(i));
                    if (breakout)
                        break;

                    breakout = true;
                }
            }
            audio.source.PlayOneShot(audio.c);
        }

        yield return new WaitForSeconds(0.40f);

        timerounds = 0;


        yield return new WaitForSeconds(timeBetweenRounds);
        while (requestspace)
            yield return new WaitForEndOfFrame();
        ResetGame();

        wait = false;

    }
    IEnumerator Plus(int i)
    {
        GameObject child = dices[i].transform.GetChild(0).gameObject;
        child.SetActive(true);

        float elapsed = 0.0f;

        while (elapsed < 0.5f)
        {

            elapsed += Time.deltaTime;
            

            child.transform.localPosition = new Vector3(0, 40 + animation.Evaluate(elapsed * 4 / 0.5f), 0);
            yield return null;
        }
        child.SetActive(false);

    }

    private void ResetGame()
    {
        if (value != target || indexes.Contains(target - 1))
            score = 0;
        else
            timePerDice -= 0.005f;

        for (int i = 0; i < 6; i++)
                dices[i].GetComponent<Image>().sprite = notEvilDices[i];

        int prev = target;

        value = 0;
        target = 0;

        bool breakout = false;
        if (score != 0)
        {
            if (!(prev == target && evilDices.Count > 1))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (evilDices.Contains(i))
                        continue;
                    if (i + 1 == prev)
                        continue;

                    if (!indexes.Contains(i))
                    {
                        target += i;
                        target++;

                        if (breakout)
                            break;

                        breakout = true;
                    }
                }
            }
        }

            if (score == 0)
        {
            target = 10;
            timePerDice = 0.2f;

        }

        if (prev == target && evilDices.Count > 1)
            target = 6;
        evilDices = new List<int>();

        if (prev == target && target == 5 && score != 0)
        {
            foreach(int dice in indexes)
            {
                dices[dice].GetComponent<Image>().sprite = evilDice;
                evilDices.Add(dice);
            }
        }

        if (target <= 6) {
            dices[target - 1].GetComponent<Image>().sprite = evilDice;
            evilDices.Add(target - 1);
        }
        dice = 0;

        for (int i = 0; i < 6; i++)
        {
            dices[i].transform.position = dicesStartingPosition[i];
            dices[i].transform.rotation = Quaternion.identity;
            dices[i].angularVelocity = 0;
            dices[i].velocity = Vector2.zero;
            dices[i].GetComponent<BoxCollider2D>().enabled = false;


        }

        indexes = new List<int>();

        if (target == 5)
        {
            ui.title.sprite = ui.titles[1];
            ui.sparks.SetActive(true);
        }
        else
        {
            ui.title.sprite = ui.titles[0];
            ui.sparks.SetActive(false);
        }
    }
}

[System.Serializable]
public struct UI
{
    public TextMeshProUGUI target;
    public TextMeshProUGUI score;

    public TextMeshProUGUI value;

    public Image space;
    public Sprite[] spaces;

    public Image title;
    public GameObject sparks;
    public Sprite[] titles;

    public int scoreSize;
    public int scoreSizeStep;
}

[System.Serializable]
public struct Audio
{
        public AudioSource source;
        public AudioClip a;
        public AudioClip b;
    public AudioClip c;
}