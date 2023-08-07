using UnityEngine;

public class LaserGun : MonoBehaviour
{
    public GameObject Laser;
    public GameObject Canvas;
    private GameObject laser;
    private Vector3[] linePoints = new Vector3[2];
    private LineRenderer lineRenderer;
    public CanvasServer canvas;
    public bool DrawFromGun;

    public void PointTheLaser(Ray ray)
    {
        lineRenderer.enabled = true;
        RaycastHit hit;

        // lineRenderer : �`��n�_�̋L�q�̎d��
        // Vector3�̔z��ŋL�q���Ȃ��Ǝg���Ȃ�
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            linePoints[1] = hit.point;
            lineRenderer.SetPositions(linePoints);
            if (DrawFromGun)
            {
                canvas.DrawFromGunAction(hit.point);
            }
        }
        else
        {
            linePoints[1] = ray.direction.normalized * 100f;
            lineRenderer.SetPositions(linePoints);
        }
    }

    public void CalculateGunRotate()
    {
        // Screen���W��(0,0)�`(Screen.width, Screen.height)����
        // (-Screen.width/2, -Screen.height/2)�`(-Screen.width/2, -Screen.height/2)�ɕϊ�����
        float half_w = Screen.width * 0.5f;
        float half_h = Screen.height * 0.5f;
        float x = Input.mousePosition.y - half_h;
        float y = Input.mousePosition.x - half_w;

        // x�� : �c�����̉�], y�� : �������̉�]
        Vector3 newAngle = Vector3.zero;
        newAngle.x = -x / half_h * 90f;
        newAngle.y = y / half_w * 90f;

        transform.rotation = Quaternion.Euler(newAngle);
    }

    void Start()
    {
        // Quaternion.identity:
        //�u��]���Ȃ��v���Ӗ�����B���̃I�u�W�F�N�g��World���e�I�u�W�F�N�g�̍��W���Ɋ��S�ɏ]��
        laser = (GameObject)Instantiate(Laser, new Vector3(0, 0, 0), Quaternion.identity);
        lineRenderer = laser.GetComponent<LineRenderer>();

        //���[�U�[�̕�
        lineRenderer.startWidth = 0.4f;
        lineRenderer.endWidth = 0.4f;
    }

    void Update()
    {
        // ���[�U�[�|�C���^�[��\������
        Ray ray = new Ray(transform.position, transform.forward);
        linePoints[0] = transform.position + transform.forward * transform.localScale.z * 0.5f;

        if (Input.GetKey("space"))
        {
            PointTheLaser(ray);
        }
        else
        {
            lineRenderer.enabled = false;
        }

        // testGun�̌������}�E�X�ɍ��킹�ĕύX����
        CalculateGunRotate();
    }
}
