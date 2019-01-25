using System;

// @TEACHING: having to start with sorting rendering, world generation, and layers seems pretty rough.
//            Probably much better to start with a screen based thing where the rendering can just go over
//            each position and check it against a few entities.

// @TODO: make EntityDatabase and Systems non-global so we have them in the local window when debugging program
namespace ASCII_RPG
{

    
    public class DynamicArray<T> {
        T[] items;
        int capacity;

        public int count;

        public T this[int i] {
            get { return items[i]; }
            set { items[i] = value; }
        }

        void EnsureCapacity() {
            if (count + 1 > capacity) {
                capacity *= 2;

                items = new T[capacity];
            }
        }

        public void Clear() {
            count = 0;
        }

        public void PushBack(T item) {
            EnsureCapacity();

            items[count] = item;
            count++;
        }

        public T PopBack() {
            count--;
            return items[count];
        }

        public DynamicArray(int capacity_) {
            capacity = capacity_;
            items = new T[capacity];
        }
    }


    // What if you cant die? Get knocked unconscious and have the world go on without you. Like MountAndBlade
    // Sometimes you just get knocked out for longer. Maybe you can lose your memory, so it isnt just a
    // roguelike with progression, or a regular roguelike where the inability to die is only narrative
    // It should be a race, dragonball style to retrieve some items. Whoever gets them has an affect on the world
    // Different factions/enemies do different things with them.
    // Generate different items.
    // Generate wishes that people make.
    // Maybe rather than doing the wishes we just say that the item does a thing (but then it doesnt watter
    // who gets it so that isnt as interesting...)
    //    Like lets say you want to prevent A from getting it because you dont like what they'd do,
    //    but you dont really mind what B would do.
    // What would it do for you tho?
    // "I wish that X was Y"
    // "I wish I had X"
    // "I wish that X didnt have Y"
    // YOU WERE THE LAST WISH
    // Merlin style living backwards? But then are people making wishes about how things would have been
    //    in the past? It doesnt make sense if there wish happens in the past but has no effect on present...
    //    Except if you're living backwards and they say "I wish I had been born a woman" then you'd meet
    //    that same person as a woman in the past.
    //    If there was a monkey-paw consequence tho maybe it would work, like "I wish there was enough water
    //    in my village" creates a drought across the land. But then it would have done that in the past right?
    //    But maybe the village had enough water in the past also?
    // You keep running into characters who havent met you yet. No one ever remembers you!
    //     Except Arthur did know merlin as a kid, its just that Merlin hasnt lived that yet? He's learning from
    //     old arthur what he should teach young arthur, even tho it wont change old arthur because that's already
    //     happened.
    // Could be a puzzle element about what you're supposed to wish for.
    // This all seems very complicated, and I could just say that you were the Last Wish and are living
    // into the future. Tho I still like the idea of descending into the past.
    // Wait tho, if people are making wishes for the future, then you already know what those wishes were
    //    (or at least you can guess), so maybe you know the dire consequences of their wish, and can try
    //    to thwart them. Part of the puzzle is deciphering who got the wish, and maybe rearranging who should
    //    get it now. Of course you wont see the consequences of the wish going into the past.
    //    UGH this is probably more complicated than its worth. The game is not really about causality,
    //    its about competition and wish making.

    // @TODO: So that we can reuse indices and entityIDs we'll need to reference entities
    // with handles.
    // Keep an index for each type in the entity struct so we can get instant access to every component
    struct Entity {
        public int id;

        public int generation;

        public Entity(int id_) {
            id = id_;
            generation = 0;
        }

        public void InvalidEntity() {
            id = 0;
            generation = 0;
        }
    };

    // @NOTE: for every new component we define we need to add it here.
    enum ComponentType {
        Renderable,
        Collidable,
        Flammable,

        Count,
    };

    struct ComponentReferences {
        public int[] references;

        public void Init() {
            references = new int[(int)ComponentType.Count];
        }

        public void Clear() {
            for (int i = 0; i < (int)ComponentType.Count; i++) {
                references[i] = -1;
            }
        }

        public bool IsReferenceValid(int reference) {
            return references[reference] >= 0;
        }
    };

    static class EntityDatabase {
        static int nextID = 1;
        static int entityCount;
        static DynamicArray<int> freeList = new DynamicArray<int>(256);

        public static DynamicArray<Entity> entities = new DynamicArray<Entity>(1024);

        // public static EntityDatabase() {
        //     nextID = 1;
        //     entityCount = 0;
        //     freeList = new DynamicArray<int>(256);
        //     entities = new DynamicArray<Entity>(1024);
        // }
        
        public static bool IsEntityValid(Entity e) {
            return e.generation == entities[e.id].generation;
        }

        public static void AddEntity() {
            entityCount++;
            
            int index = 0;
            if (freeList.count > 0) {
                index = freeList.PopBack();
            }
            else {
                index = nextID;
                nextID++;
            }

            entities[index] = new Entity(index);
        }

        public static void DeleteEntity(Entity e) {
            entityCount--;

            // @TODO: defer this! Need to remove all its components!!!
            if (IsEntityValid(e)) {
                freeList.PushBack(e.id);

                e.generation++;

                // @NOTE: we cant do entities[e.id].generation++ because its not a reference
                entities[e.id] = e;
            }
        }
    }

    // The difference between structs and classes are that classes are heap allocated
    // and passed by reference. Structs are pass by value and usually allocated on the stack
    
    struct Vector2 {
        public int x;
        public int y;

        public Vector2(int x_, int y_) {
            x = x_;
            y = y_;
        }
        
        public Vector2 Add(Vector2 a) {
            Vector2 result;
            result.x = x + a.x;
            result.y = y + a.y;
            return result;
        }
    };

    struct Box {
        public Vector2 min;
        public Vector2 max;

        // @NOTE: INCLUSIVE
        public bool PointInBox(Vector2 point) {
            bool result = false;

            result = (point.x >= min.x && point.x <= max.x) &&
                (point.y >= min.y && point.y <= max.y);

            return result;
        }

        public bool BoxTest(Box boxB) {
            bool collide = false;

            collide = this.PointInBox(boxB.min) || this.PointInBox(boxB.max);

            return true;
        }

        public Box(int size) {
            min.x = -size;
            min.y = -size;
            max.x = size;
            max.y = size;
        }

        public Box(Vector2 position, int size) {
            min.x = -size;
            min.y = -size;
            max.x = size;
            max.y = size;

            min = min.Add(position);
            max = max.Add(position);
        }

    };

    struct Camera {
        // @NOTE: this box is relative to the players position
        // and gives us dimensions for what to render
        public Box box;
        public Vector2 position;
    };

    
    class Positions {
        public DynamicArray<Vector2> positions;
        public DynamicArray<int> freeList;

        public Positions() {
            positions = new DynamicArray<Vector2>(1024);
            freeList = new DynamicArray<int>(1024);
        }

        public void Add(Vector2 pos) {
            positions.PushBack(pos);
        }

        public void Remove(Entity e) {
            freeList.PushBack(e.id);
        }
    }
    
    struct Renderable {
        public int symbol;
        public ConsoleColor color;
        public int layer;

        public Renderable(int n, ConsoleColor c, int l) {
            symbol = n;
            color = c;
            layer = l;
        }
    };

    class Renderables {
        public DynamicArray<Renderable> renderables;
        public DynamicArray<int> freeList;

        public Renderables () {
            renderables = new DynamicArray<Renderable>(1024);
            freeList = new DynamicArray<int>(1024);
        }

        public void Add(Renderable pos) {
            renderables.PushBack(pos);
        }

        public void Remove(Entity e) {
            freeList.PushBack(e.id);
        }        
    }

    struct Collidable {
        bool collides;
    };

    struct Flammable {
        
    };

    struct Grass {
        public Grass(Vector2 pos) {
            EntityDatabase.AddEntity();

            if (pos.x % 2 == 0) {
                Systems.renderables.Add(new Renderable((int)'/', ConsoleColor.DarkGreen, 0));
            }
            else {
                Systems.renderables.Add(new Renderable((int)'/', ConsoleColor.Green, 0));
            }

            Systems.positions.Add(pos);
        }
    };

    struct Player {
        public Player(Vector2 pos) {
            Systems.renderables.Add(new Renderable((int)'@', ConsoleColor.White, 1));
            Systems.positions.Add(pos);
        }
    }

    static class Systems {
        public static Positions positions = new Positions();
        public static Renderables renderables = new Renderables();
        
    }
    
    // BOTW
    // Elements and materials
    // Elements can change a materials state
    // Elements can change an elements state
    // Materials cant change other materials

    // Combine runes together to create spells
    // Many moves will have "activate" and "target"

    class Program {

        static Player player = new Player(new Vector2(2, 2));

        static Grass[] grass = new Grass[20 * 20];
        static void GenerateWorld() {
            for (int y = 0; y < 20; y++) {
                for (int x = 0; x < 20; x++) {
                    Vector2 position = new Vector2(x, y);
                    grass[y + (20 * x)] = new Grass(position);
                }
            }
        }


        static Camera camera;
        static int cameraHalfSize = 4;
        static int cameraSize = (cameraHalfSize * 2);
        static int cameraPitch = cameraSize + 1;
        static int renderableEntities = (cameraPitch * cameraPitch);

        // * 2 to assume that there are two things in every spot, even tho there rarely are, we likely
        // will never have to resize this DynamicArray
        static DynamicArray<Renderable> renderables = new DynamicArray<Renderable>(renderableEntities * 2);
        static DynamicArray<Vector2> renderablePositions = new DynamicArray<Vector2>(renderableEntities * 2);
        static Renderable[] renderablesSorted = new Renderable[renderableEntities];
        static Vector2[] renderablePositionsSorted = new Vector2[renderableEntities];

        static void CollectRenderables() {
            renderables.Clear();
            renderablePositions.Clear();

            Box cameraWorldBox = new Box(camera.position, cameraHalfSize);
            
            for (int i = 0; i < Systems.renderables.renderables.count; i++) {
                // @NOTE: assuming everything renderable has a position, so why not just group them together dummy
                // @TODO: need to resolve two things in the same spot
                if (cameraWorldBox.PointInBox(Systems.positions.positions[i])) {
                    renderables.PushBack(Systems.renderables.renderables[i]);
                    renderablePositions.PushBack(Systems.positions.positions[i]);
                }
            }

            // @PERF: this is VERY BAD. If we're rendering 100 entities we do this potentialy 100^2 times!!!
            // What are some ways we could make it better?
            // @TODO: sort the renderables!
            Vector2 currPosition = new Vector2(cameraWorldBox.min.x, cameraWorldBox.min.y);
            // @NOTE: this is relative to cameraWorldBox.min, and we use it to index into the renderables
            //        when overwriting a previous entry because of depth issues.
            Vector2 currPositionRelative = new Vector2(0, 0);
            int r = 0;
            while (r < renderableEntities) {

                bool foundRenderableAtPosition = false;
                for (int i = 0; i < renderables.count; i++) {

                    // @PERF: because multiple things could be at the same position we have to check EVERYTHING
                    // to make sure we pick the thing with the topmost layer, which is very unfortunate
                    if (currPosition.Equals(renderablePositions[i])) {

                        if (foundRenderableAtPosition) {
                            if (renderablesSorted[r].layer < renderables[i].layer) {
                                renderablesSorted[r] = renderables[i];
                                renderablePositionsSorted[r] = renderablePositions[i];
                            }
                        }
                        else {
                            renderablePositionsSorted[r] = renderablePositions[i];
                            renderablesSorted[r] = renderables[i];
                            foundRenderableAtPosition = true;
                        }
                    }
                }
                
                if (foundRenderableAtPosition) {
                    r++;
                        
                    currPositionRelative.x++;
                    currPosition.x++;
                    if (currPosition.x > cameraWorldBox.max.x) {
                        currPosition.x = cameraWorldBox.min.x;
                        currPosition.y++;

                        currPositionRelative.x = 0;
                        currPositionRelative.y++;
                    }
                }
            }
        }

        static void RenderWorld() {
            int x = 0;
            for (int i = 0; i < renderableEntities; i++) {

                Console.ForegroundColor = renderablesSorted[i].color;
                Console.Write((char)renderablesSorted[i].symbol);

                x++;

                if (x == cameraPitch) {
                    x = 0;
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
        }

        static void UpdatePlayer(String key) {
            switch (key) {
                case "W" : {
                    Systems.positions.positions[0] = new Vector2(Systems.positions.positions[0].x,
                                                                 Systems.positions.positions[0].y - 1);
                } break;

                case "S" : {
                    Systems.positions.positions[0] = new Vector2(Systems.positions.positions[0].x,
                                                                 Systems.positions.positions[0].y + 1);
                } break;

                case "A" : {
                    Systems.positions.positions[0] = new Vector2(Systems.positions.positions[0].x - 1,
                                                                 Systems.positions.positions[0].y);
                } break;

                case "D" : {
                    Systems.positions.positions[0] = new Vector2(Systems.positions.positions[0].x + 1,
                                                                 Systems.positions.positions[0].y);
                } break;
            }
        }

        
        
        static void Main(string[] args) {
            
            camera.box = new Box(cameraHalfSize);
            camera.position = new Vector2(cameraHalfSize, cameraHalfSize);
            
            GenerateWorld();

            while (true) {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                String key = keyInfo.Key.ToString();

                UpdatePlayer(key);
                // @TODO: only do this if we can fit into the corner
                // @TODO: actually I'd rather implement world wrapping for design reasons anyway
                // @TODO: for world wrapping I dont want top Y to lead to bottom Y, but map like on a sphere
                //        Honestly tho that might not really matter
                //camera.position = Systems.positions.positions[0];
                
                CollectRenderables();

                // @NOTE: dont clear until we have renderables so there isnt a blankscreen
                // while we do the search (it should be really fast anyway but just in case)
                Console.Clear();
                
                RenderWorld();

                
            }
            
            // Console.BackgroundColor = ConsoleColor.Blue;
            // Console.BackgroundColor = ConsoleColor.Red;
        }
    }
}
