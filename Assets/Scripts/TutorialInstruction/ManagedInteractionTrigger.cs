using UnityEngine;

public class ManagedInteractionTrigger : MonoBehaviour
{
   
    [TextArea(2, 4)]
    public string promptText = "Press Tab To Equip Tool";

  
    [Range(0, 10)]
    public int priority = 1;


    public string playerTag = "Player";

   
    public bool showOnEnter = true;

 
    public bool hideOnExit = true;

    public bool enableKeyDetection = false;


    public KeyCode interactionKey = KeyCode.Tab;

 
    public UnityEngine.Events.UnityEvent onPlayerEnter;

  
    public UnityEngine.Events.UnityEvent onPlayerExit;

 
    public UnityEngine.Events.UnityEvent onInteractionKeyPressed;

    private bool isPlayerInRange = false;

    private void Start()
    {
       
        Collider col = GetComponent<Collider>();
       
    }

    private void Update()
    {
       
        if (enableKeyDetection && isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            onInteractionKeyPressed?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;

            if (showOnEnter && InteractionPromptManager.Instance != null)
            {
                InteractionPromptManager.Instance.ShowPrompt(promptText, priority);
            }

            onPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;

            if (hideOnExit && InteractionPromptManager.Instance != null)
            {
                InteractionPromptManager.Instance.HidePrompt(priority);
            }

            onPlayerExit?.Invoke();
        
        }
    }



    public void ShowPrompt()
    {
        if (InteractionPromptManager.Instance != null)
        {
            InteractionPromptManager.Instance.ShowPrompt(promptText, priority);
        }
    }


    public void HidePrompt()
    {
        if (InteractionPromptManager.Instance != null)
        {
            InteractionPromptManager.Instance.HidePrompt(priority);
        }
    }


    public void UpdatePromptText(string newText)
    {
        promptText = newText;

      
        if (isPlayerInRange && InteractionPromptManager.Instance != null)
        {
            InteractionPromptManager.Instance.ShowPrompt(promptText, priority);
        }
    }

 
    private void OnDrawGizmos()
    {
        Gizmos.color = isPlayerInRange ? Color.green : Color.yellow;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}

