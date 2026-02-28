using UnityEngine;

public class CloudParallax : MonoBehaviour
{
    [Header("Configuração de Velocidade")]
    [Tooltip("Valores positivos movem para a direita, negativos para a esquerda.")]
    public float speed = 0.5f;

    [Header("Limites do Cenário")]
    [Tooltip("Quando a nuvem passar do limite ela teleporta para o ponto inicial")]
    public float startX = -10f;
    public float endX = 10f;

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (speed > 0f && transform.position.x > endX)
        {
            var pos = transform.position;
            pos.x = startX;
            transform.position = pos;
        }
        else if (speed < 0f && transform.position.x < startX)
        {
            var pos = transform.position;
            pos.x = endX;
            transform.position = pos;
        }
    }
}
