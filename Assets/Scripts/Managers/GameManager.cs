using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 10;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public CameraControl m_CameraControl;
    public Text m_MessageText;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;


    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;



    public Text timerText;
    public Text textoGanar;
    public float timer = 0.00f;
    public float timerToEnd = 0.00f;
    //
    private bool timeIsRunning = false;
    private bool timerToEndIsRunning = true;
    private bool empate = false;


    private void Start()
    {
        timerText.text ="";
        textoGanar.text = "";

        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);


        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }

    void Update()
    {
        if(timeIsRunning)
        {
            if(timer >= 0)
            {
                timer += Time.deltaTime;
                DisplayTime(timer);
            }
        }   

        

        if(timerToEndIsRunning)
        {
            if(timerToEnd >= 0)
            {
                timerToEnd += Time.deltaTime;
                DisplaytimerToEnd(timerToEnd);
            } 
        } 

        if(empate)
        {

        }   
    }


    void DisplayTime(float timeToDisplay)
    {
        textoGanar.text = "Empate";
        timeToDisplay += 1;
        float seconds = Mathf.FloorToInt(timeToDisplay %60);
        //timerText.text = seconds.ToString();
        if(seconds == 5)
        {
            SceneManager.LoadScene("Principal");
        }
    }

    void DisplaytimerToEnd(float timerToEndDisplay)
    {
        
        float seconds = Mathf.FloorToInt(timerToEndDisplay %60);
        timerText.text = (60 - seconds).ToString();
        if(seconds >= 59)
        {
            empate = true;
            timerToEndIsRunning = false;
            timeIsRunning= true;
        } 

        timerToEndDisplay += 1;
    }


    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl();

        m_MessageText.text = string.Empty;

        while (!OneTankLeft())
        {
            yield return null;
        }
    }


    private IEnumerator RoundEnding()
    {
        // Deshabilito el movimiento de los tanques. 
        DisableTankControl();

        // Borro al ganador de la ronda anterior. 
        m_RoundWinner = null;

        // Miro si hay un ganador de la ronda. 
        m_RoundWinner = GetRoundWinner();

        // Si lo hay, incremento su puntuación. 
        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;

        // Compruebo si alguien ha ganado el juego. 
        m_GameWinner = GetGameWinner();

        // Genero el mensaje según si hay un gaandor del juego o no. 
        string message = EndMessage();
        m_MessageText.text = message;

        // Espero a que pase el tiempo de espera antes de volver al bucle. 
        yield return m_EndWait;
    }

    // Usado para comprobar si queda más de un tanque. 
    private bool OneTankLeft()
    {
        // Contador de tanques. 
        int numTanksLeft = 0;

        // recorro los tanques... 
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si está activo, incremento el contador. 
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        // Devuelvo true si queda 1 o menos, false si queda más de uno. 
        return numTanksLeft <= 1;
    }

    // Comprueba si algún tanque ha ganado la ronda (si queda un tanque o meno s). 
    private TankManager GetRoundWinner()
    {
        // Recorro los tanques... 
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si solo queda uno, es el ganador y lo devuelvo. 
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        // SI no hay ninguno activo es un empate, así que devuelvo null. 
        return null;
    }

    // Comprueba si hay algún ganador del juegoe. 
    private TankManager GetGameWinner()
    {
        // Recorro los tanques... 
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si alguno tiene las rondas necesarias, ha ganado y lo devuelvo. 
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
            {
                timerToEndIsRunning = false;
                return m_Tanks[i];
            }
        }

        // Si no, devuelvo null. 
        return null;
    }

    // Deveulve el texto del mensaje a mostrar al final de cada ronda. 
    private string EndMessage()
    {
        // Pordefecto no hya ganadores, así que es empate. 
        string message = "EMPATE!";

        // Si hay un ganador de ronda cambio el mensaje. 
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        // Retornos de carro. 
        message += "\n\n\n\n";

        // Recorro los tanques y añado sus puntuaciones. 
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";
        }

        // Si hay un ganador del juego, cambio el mensaje entero para reflejarlo. 
        if (m_GameWinner != null)
        {
            message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!"  + "\n\n Con " + m_RoundWinner.m_Wins + " Puntos";
        }

        return message;
    }

    // Para resetear los tanques (propiedaes, posiciones, etc.). 
    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }

    //Habilita el control del tanque 
    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }

    //Deshabilita el control del tanque 
    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}