using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using UnityEngine.UIElements;

/*Straight Shooter fires their weapon in a horizontal direction. The direction can be changed by re-entering the
coordinates of where the unit was placed. The weapon's range is 2 spaces.
Enhanced Unit: unlimited weapon range, and weapon pierces targets. */
public class PlayerUnit_StraightShooter : PlayerUnit
{
    public Bullet bullet;
    //public Vector2 bulletRange;
    private float cooldownTimer;            //tracks when weapon fires.
    [SerializeField]private bool reverseDirection;  //if true, unit fires left.

    Coroutine rotateCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        //bullet setup
        GameManager gm = Singleton.instance.GameManager;
        //GetUnitData(unitData);
        
        bullet = Instantiate(gm.bulletPrefab, parent: this.transform);
        SetupBullet(bullet);
        bullet.gameObject.SetActive(false);

        //range of bullet is the unit's position + 2 units
        //bulletRange = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y);
        SetBulletRange(bullet);

        cooldownTimer = 0;
        //EnhanceUnit();
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;

        base.Update();    
        //fire weapon
        if (bullet == null)     //this is to prevent game from updating before bullet exists.
            return;

        if (!bullet.gameObject.activeSelf && Time.time > cooldownTimer + weaponCooldown)
        {
            //cooldownTimer = Time.time;
            bullet.gameObject.SetActive(true);

            //Extra step in case weapon cooldown was altered due to piercing bullet getting blocked by something.
            if (isEnhanced && weaponCooldown > 0)
                weaponCooldown = 0;
        }
        else
        {
            bulletRange = SetBulletRange(bullet);
            float distance = Vector2.Distance(bulletRange,  bullet.transform.position);
            Logger.DebugLog("Straight Shooter Bullet distance to target: " + distance);
            //if (bullet.transform.position.x > bulletRange.x)
            if (distance < 0.5f || bullet.bulletStruckUnit)
            {
                cooldownTimer = Time.time;
                bullet.bulletStruckUnit = false;

                //check if unit's firing direction needs to change.
                if (reverseDirection == true)
                {
                    reverseDirection = false;
                    bullet.direction *= -1;
                    rotateCoroutine = StartCoroutine(RotateUnit());
                    //unitSpriteRenderer.flipX = !unitSpriteRenderer.flipX;
                    //SetBulletRange(bullet);
                    //bulletRange = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y);
                }
                //Logger.DebugLog("Bullet range: " + bulletRange);
                bullet.transform.position = transform.position;
                bullet.gameObject.SetActive(false);

            }
        }
    }

    public override void SetupBullet(Bullet bullet)
    {
        base.SetupBullet(bullet);
        //bullet.moveSpeed = 6;
        bullet.direction = Vector2.right;
        bullet.owner = Bullet.Owner.Player;
        bullet.GetComponent<SpriteRenderer>().sortingOrder = 0;   //bullet appears behind unit.
        //bullet.damage = 1;
        //bullet.distance = 4;
    }

    //change firing direction.
    public override void ActivateAbility()
    {
        GameManager gm = Singleton.instance.GameManager;
        gm.DisplayAlertWithDuration(GetText("reverseFireDirection"));
        Logger.DebugLog("Reversing Unit firing direction!");
        reverseDirection = true;

        //rotate unit on Z axis and flip sprite on Y axis so that shadow appears correct.
        //rotateCoroutine = StartCoroutine(RotateUnit());
    }

    //Extend bullet range and grant piercing effect.
    public override void EnhanceUnit()
    {
        if (isEnhanced)
            return;

        base.EnhanceUnit();
        isEnhanced = true;

        weaponCooldown = 0;         //to compensate for the extended range and only 1 bullet fired at a time.
        bullet.bulletType = Bullet.BulletType.Pierce;
        bullet.distance = 10;       //should be large enough to go off screen.
        SetBulletRange(bullet);
        //bulletRange = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y);
        Logger.DebugLog("Unit is enhanced");
    }

    //Used for when bullet hits barrier.
    public void SetCooldown()
    {
        cooldownTimer = Time.time;
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    /* Rotate unit on its Z axis until it reaches 180 degrees. Halfway through the rotation, the sprite's Y
    axis is flipped so the shadow appears at the bottom. */
    IEnumerator RotateUnit()
    {
        float zTarget = 180;
        float halfwayPoint = 90;
        float rotateSpeed = 1000;

        if (unitSpriteRenderer.transform.localEulerAngles.z == 0)
        {
            while(unitSpriteRenderer.transform.localEulerAngles.z < zTarget)
            {
                Vector3 currentAngle = unitSpriteRenderer.transform.localEulerAngles;
                unitSpriteRenderer.transform.localEulerAngles = new Vector3(0, 0, currentAngle.z + rotateSpeed * Time.deltaTime);

                //check if sprite needs flipping
                if (unitSpriteRenderer.flipY == false && unitSpriteRenderer.transform.localEulerAngles.z >= halfwayPoint)
                {
                    unitSpriteRenderer.flipY = true;
                }
                yield return null;
            }

            unitSpriteRenderer.transform.localEulerAngles = new Vector3(0, 0, zTarget); //unsure why Z value is always negative.
            if (unitSpriteRenderer.transform.localEulerAngles.z < 0)
                unitSpriteRenderer.transform.localEulerAngles *= -1;
        }
        else
        {
            //reset angle to 0
            while(unitSpriteRenderer.transform.localEulerAngles.z > 0)
            {
                Vector3 currentAngle = unitSpriteRenderer.transform.localEulerAngles;
                unitSpriteRenderer.transform.localEulerAngles = new Vector3(0, 0, currentAngle.z - rotateSpeed * Time.deltaTime);

                //check if sprite needs flipping
                if (unitSpriteRenderer.flipY == true && unitSpriteRenderer.transform.localEulerAngles.z <= halfwayPoint)
                {
                    unitSpriteRenderer.flipY = false;
                }

                //must check if approaching 0 and not overshoot
                if (Vector3.Distance(unitSpriteRenderer.transform.localEulerAngles, Vector3.zero) <= 10f)
                    unitSpriteRenderer.transform.localEulerAngles = Vector3.zero;
                yield return null;
            }
            //transform.localEulerAngles = Vector3.zero;
        }
    }
}
