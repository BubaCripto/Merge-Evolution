using UnityEngine;

public class CloudParallax : MonoBehaviour
{
    [Header("Configuração de Velocidade")]
    [Tooltip("Valores positivos movem para a direita, negativos para a esquerda.")]
    public float speed = 0.5f;

    [Header("Limites do Cenário")]
    [Tooltip("Quando a nuvem passar do endX, ela teleporta de volta para o startX")]
    public float startX = -10f;
    public float endX = 10f;

    void Update()
    {
        // Move a nuvem suavemente a cada frame
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Se estiver indo para direita e passou do limite
        if (speed > 0 && transform.position.x > endX)
        {
            Vector3 pos = transform.position;
            pos.x = startX;
            transform.position = pos;
        }
        // Se estiver indo para a esquerda e passou do limite
        else if (speed < 0 && transform.position.x < endX)
        {
            Vector3 pos = transform.position;
            pos.x = startX;
            transform.position = pos;
        }
    }
}
