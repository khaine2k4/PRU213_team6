using UnityEngine;

public class Enemy : MonoBehaviour
{
         [SerializeField] private float speed = 5f;
         [SerializeField] private float distance = 5f;
        private  Vector3 startPos ;
        private bool movingRight = true;  
        private Rigidbody2D rb;
        private Animator animator;
        private GameManager gameManager;    
      
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
      
    }

    // Update is called once per frame
    void Update()
    {
        float leftBound = startPos.x - distance;
        float rightBound = startPos.x + distance;
        if( movingRight)
          {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
            if(transform.position.x >= rightBound)
            {
                movingRight = false;
                Flip(); 
            }
          } 
          else
          {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
            if(transform.position.x <= leftBound)
            {
                movingRight = true;
                Flip();
            }
          }   

          void Flip(){
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
          }  

    }
}
