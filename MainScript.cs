using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassSpringSystem : MonoBehaviour {
    //public Material Material; 

   
    private const int anchorLimit = 2;


    private const int rows = 10; //number of rows of grabbable points 
    private const int col = 10; //number of columns of grabbable points 
    private const float pointScale = .3f; //size of grabbable points
    private const float mass = 0.07f; //mass of cloth points 
    private const float ks = 7f; //spring stiffness
    private const float lengthBuffer = 0.05f;
    private const float lsider = 0.5f; //side rest length
    private float lhypr; //hypotenuse rest length
    private const float kd = 0.5f; //damping constant 
    private const float r = 0.8f; 
    private const float deltaT = 0.06f;
    private Vector3 g = new Vector3(0, -9.8f, 0); //gravity

    private Queue<GameObject> anchors = new Queue<GameObject>(); //points unaffected by gravity 
    private Mesh clothMeshFront;
    private GameObject clothFront;
    private Mesh clothMeshBack;
    private GameObject clothBack;

    private Vector3 Fg; //gravity force 

    private GameObject[,] points = new GameObject[rows,col]; //grabbable points 
    private Vector3[,] verticesPos = new Vector3[rows, col]; //positions of points
    private Vector3[,] verticesVel = new Vector3[rows, col]; //velocities of points 


    // Use this for initialization
    void Start () {
        //calculate hyponenuse length 
        lhypr = Mathf.Sqrt(2f * (lsider * lsider));

        //create all grabbable points
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < col; j++)
            {
                addObject(i, j, new Vector3(0, -lsider*i, lsider*j));
            }
        }

        //set first anchor 
        anchors.Enqueue(points[0, 0]);

        //calculate gravity force 
        Fg = mass * g;

        //create cloth mesh 
        clothMeshFront = new Mesh();
        clothMeshBack = new Mesh();
        Vector3[] vertices = new Vector3[rows * col];
        makeVertices(vertices);
        clothMeshFront.vertices = vertices;
        clothMeshBack.vertices = vertices;
        Vector2[] uvs = new Vector2[rows * col];
        makeUVs(vertices, uvs);
        clothMeshFront.uv = uvs;
        clothMeshBack.uv = uvs;
        int[] trianglesFront = new int[(rows - 1) * (col - 1) * 6];
        makeFrontTriangles(trianglesFront);
        clothMeshFront.triangles = trianglesFront;
        int[] trianglesBack = new int[(rows - 1) * (col - 1) * 6];
        makeBackTriangles(trianglesBack);
        clothMeshBack.triangles = trianglesBack;

        clothFront = new GameObject("ClothFront", typeof(MeshFilter), typeof(MeshRenderer));
        clothFront.GetComponent<MeshFilter>().mesh = clothMeshFront;
        clothFront.GetComponent<MeshRenderer>().material.color = Color.blue;
        clothBack = new GameObject("ClothBack", typeof(MeshFilter), typeof(MeshRenderer));
        clothBack.GetComponent<MeshFilter>().mesh = clothMeshBack;
        clothBack.GetComponent<MeshRenderer>().material.color = Color.blue;
    }
	
	void Update () {
            //do the math for all the points' positions 
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < col - 1; j++)
                {
                    handlePair(i, j, i + 1, j, lsider);
                    handlePair(i, j, i, j+1,lsider);
                    handlePair(i, j, i+1, j+1,lhypr);
                    handlePair(i+1, j, i, j+1,lhypr);
                }
            }
            for (int i = 0; i < rows - 1; i++)
            {
                handlePair(i, col-1, i+1, col-1,lsider);
            }
            for (int j = 0; j < col - 1; j++)
            {
                handlePair(rows-1, j, rows-1, j+1,lsider);
            }
            
            //reset mesh points 
            Vector3[] vertices = new Vector3[rows * col];
            makeVertices(vertices);
            clothMeshFront.vertices = vertices;
            clothMeshBack.vertices = vertices;
            Vector2[] uvs = new Vector2[rows * col];
            makeUVs(vertices, uvs);
            clothMeshFront.uv = uvs;
            clothMeshBack.uv = uvs;
            int[] trianglesFront = new int[(rows - 1) * (col - 1) * 6];
            makeFrontTriangles(trianglesFront);
            clothMeshFront.triangles = trianglesFront;
            int[] trianglesBack = new int[(rows - 1) * (col - 1) * 6];
            makeBackTriangles(trianglesBack);
            clothMeshBack.triangles = trianglesBack;
    }

    //make vertices for the mesh
    private void makeVertices(Vector3[] vertices)
    {
        int k = 0; 
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < col; j++)
            {
                vertices[k] = points[i, j].transform.position; 
                k++;
            }
        }
    }

    //make uv's for the mesh 
    private void makeUVs(Vector3[] vertices, Vector2[] uvs)
    {
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
    }

    //make triangles for the mesh facing front way 
    private void makeFrontTriangles(int[] triangles)
    {
        int k = 0;
        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < col - 1; j++)
            {
                triangles[k] = (i*col)+j;
                k++;
                triangles[k] = (i*col)+j + 1;
                k++;
                triangles[k] = (i * col) + j + col;
                k++;
                triangles[k] = (i * col) + j + col;
                k++;
                triangles[k] = (i * col) + j + 1;
                k++;
                triangles[k] = (i * col) + j + 1 + col;
                k++;
            }
        }
    }

    //make triangles for the mesh facing back way 
    private void makeBackTriangles(int[] triangles)
    {
        int k = 0;
        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < col - 1; j++)
            {
                triangles[k] = (i * col) + j + col;
                k++;
                triangles[k] = (i * col) + j + 1;
                k++;
                triangles[k] = (i * col) + j;
                k++;
                triangles[k] = (i * col) + j + 1 + col;
                k++;
                triangles[k] = (i * col) + j + 1;
                k++;
                triangles[k] = (i * col) + j + col;
                k++;
            }
        }
    }

    //calculate force between two points in cloth 
    private void handlePair(int ai, int aj, int bi, int bj, float length)
    {
        GameObject a = points[ai, aj];
        GameObject b = points[bi, bj];

        //for when a is being dragged
        if (!a.transform.position.Equals(verticesPos[ai, aj]) && !anchors.Contains(a))
        {
            //set new anchor
            verticesPos[ai, aj] = a.transform.position;
            verticesVel[ai, aj] = new Vector3(0, 0, 0);
            anchors.Enqueue(a);

            //make sure number of anchors stays the same 
            while (anchors.Count > anchorLimit)
                anchors.Dequeue();


        }

        //for when b is being dragged
        if (!anchors.Contains(b) && !b.transform.position.Equals(verticesPos[bi, bj]))
        {
            //set new anchor
             verticesPos[bi, bj] = b.transform.position;
             verticesVel[bi, bj] = new Vector3(0, 0, 0);
             anchors.Enqueue(b);

            //make sure number of anchors stays the same
            while (anchors.Count > anchorLimit)
                 anchors.Dequeue();

        }

        //skip calculations if both points are anchors
        if (anchors.Contains(a) && anchors.Contains(b))
            return;

        //find distance between two points 
        float dist = Vector3.Distance(a.transform.position, b.transform.position);

        //determine which point is NOT an anchor 
        GameObject nonAnchor = a;
        int nonAnchori = ai, nonAnchorj = aj;
        GameObject other = b;
        int otheri = bi, otherj = bj;
        if (anchors.Contains(a))
        {
            nonAnchor = b;
            nonAnchori = bi; nonAnchorj = bj;
            other = a;
            otheri = ai; otherj = aj;
        }

        //determine current positions and velocities 
        Vector3 nonPointPos = verticesPos[nonAnchori, nonAnchorj]; 
        Vector3 nonPointVel = verticesVel[nonAnchori, nonAnchorj];
        Vector3 otherPointPos = verticesPos[otheri, otherj];
        Vector3 otherPointVel = verticesVel[otheri, otherj];

        //calculate individual forces on definite non anchor
        Vector3 Fsd = calcMainForce(nonAnchor, other, nonPointVel, otherPointVel, length);
        Vector3 Fr = calcRForce(nonPointVel);

        //calculate full force (negate gravity if stretched too far)
        //Vector3 F = Fr + Fg + Fsd;
        Vector3 F = Fr + Fsd; 
        if(dist < length + lengthBuffer)
        {
            F += Fg; 
        }

        //calculate new position
        Vector3 accel = F / mass;
        Vector3 vel = nonPointVel + accel * deltaT;
        Vector3 pos = nonPointPos + vel * deltaT;
        nonAnchor.transform.position = pos;

        //save info for next calculation
        verticesVel[nonAnchori, nonAnchorj] = vel;
        verticesPos[nonAnchori, nonAnchorj] = pos;

        if(!anchors.Contains(other))
        {
            //calculate total force on other point 
            Fr =  calcRForce(otherPointVel);
            //F = Fr + Fg + -Fsd;
            F = Fr - Fsd;
            if (dist < length + lengthBuffer)
            {
                F += Fg;
            }

            //calculate new position for other point
            accel = F / mass;
            vel = otherPointVel + accel * deltaT;
            pos = otherPointPos + vel * deltaT;
            other.transform.position = pos;

            //save info for next calc
            verticesVel[otheri, otherj] = vel;
            verticesPos[otheri, otherj] = pos;
        }

    }

    //create new grabbable object 
    private void addObject(int i, int j, Vector3 position)
    {
        GameObject a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        a.GetComponent<MeshRenderer>().material.color = Color.blue;
        a.transform.localScale *= pointScale;
        a.transform.position = position;
        a.AddComponent<MouseDrag>();
        a.AddComponent<BoxCollider>();
        Vector3 vel = new Vector3(0, 0, 0); 
        points[i,j] = a;
        verticesPos[i, j] = position;
        verticesVel[i, j] = vel; 
    }

    //calculate spring force and damping force 
    private Vector3 calcMainForce(GameObject a, GameObject b, Vector3 aVel, Vector3 bVel, float length)
    {
        float lc = Vector3.Distance(a.transform.position, b.transform.position);
        Vector3 difference = a.transform.position - b.transform.position;
        Vector3 unitV = difference / difference.magnitude;
        Vector3 Fs = -ks * (lc - length) * unitV;
        Vector3 Fd = -kd * Vector3.Dot((aVel - bVel), unitV) * unitV;

        return Fs + Fd; 
    }

    //calculate stabilizing force 
    private Vector3 calcRForce(Vector3 vel)
    {
        return -r * vel;
    }

}
