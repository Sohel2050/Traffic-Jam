using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeBoardLevel : MonoHandler
{
    [SerializeField]
    private int width = 8;
    [SerializeField]
    private int height = 12;

    [SerializeField]
    private GameObject blockPrefab;
    [SerializeField]
    private Transform nextBlockWidth;
    [SerializeField]
    private Transform nextBlockHeight;
    [SerializeField]
    private List<Sprite> blocksSprite = new List<Sprite>();
    [SerializeField]
    private List<GameObject> blocks = new List<GameObject>();

    public List<GameObject> GetBlocks
    {
        get
        {
            return blocks;
        }
    }



    public override void GUIEditor()
    {

#if UNITY_EDITOR
        if(GUILayout.Button("Make Board"))
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                DestroyImmediate( blocks[i]);
            }
            blocks.Clear();
            float xNextBlock = nextBlockWidth.position.x - blockPrefab.transform.position.x;
            float yNextBlock = nextBlockHeight.position.y - blockPrefab.transform.position.y;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    GameObject block = Instantiate(blockPrefab, Vector3.zero, Quaternion.identity);
                    block.SetActive(true);
                    if (i % 2 == 0)
                    {
                        if (j % 2 == 0)
                        {
                            block.GetComponent<SpriteRenderer>().sprite = blocksSprite[0];
                        }
                    }
                    else
                    {
                        if (j % 2 != 0)
                        {
                            block.GetComponent<SpriteRenderer>().sprite = blocksSprite[0];
                        }
                    }
                    block.transform.SetParent(transform, false);
                    block.transform.position = new Vector3(blockPrefab.transform.position.x + (xNextBlock * j), blockPrefab.transform.position.y + (yNextBlock * i), 0);
                    block.name = string.Format("Block_{0}_{1}", i, j);
                    blocks.Add(block);
                }
            }
        }

        if (GUILayout.Button("Delete Board"))
        {

            for (int i = 0; i < blocks.Count; i++)
            {
                DestroyImmediate(blocks[i]);
            }
            blocks.Clear();
        }

        base.GUIEditor();
#endif
    }
}
