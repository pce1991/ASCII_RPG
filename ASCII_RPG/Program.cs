using System;

namespace ASCII_RPG
{

    struct Entity {
        int id;
    };

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

    class PositionSystem {
        
    }

    struct Box {
        public Vector2 min;
        public Vector2 max;
        
        public bool PointInBox(Vector2 point, Box box) {
            bool result = false;

            result = (point.x >= box.min.x && point.x <= box.max.x) &&
                (point.y >= box.min.y && point.y <= box.max.y);

            return result;
        }

        public bool BoxTest(Box boxA, Box boxB) {
            bool collide = false;

            collide = PointInBox(boxA.min, boxB) || PointInBox(boxA.max, boxB);

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
    };
    
    struct Renderable {
        int symbol;

        public Renderable(int n) {
            symbol = n;
        }
    };

    struct Collidable {
        bool collides;
    };

    struct Flammable {
        
    };

    struct Grass {
        Vector2 position;
        Renderable renderable;
        Flammable flammable;

        public Grass(Vector2 pos) {
            position = pos;
            renderable = new Renderable((int)'/');
        }
    };

    // When something moves, if that space is already occupied, and is collidable, then the move fails

    // We want each flammable thing to determine what it does when its on fire
    // Could we do this with messaging? The flames would just post a message
    // which other components would respond to? Like health would see "oh I'm buring, decrement health"
    // grass would say "change my symbol"
    // Everything else is responsible for saying: "hey, if i'm no fire, do a thing"?
    // Maybe its easiest to just do a big ol branch in the fire code? For all the types that respond to fire
    // We check to see if they have that component, and if so we burn em.
    // For performance what I'd like is to grab everything that is on fire and is also grass.
    // If we keep IDs ascending would this work?
    //    F F F F F F F F F
    //    G G P C G G C G G
    //    4 5 1 2 6 7 3 8 9
    //    But wouldnt they be put in this order then?
    //    P C C G G G G G G?
    // Can we then make the assumption that everything in flammable between the first ID of grass
    // and the last ID of grass is a grass?
    // We can create new entities, but cant add/remove components from them
    //    That means first and last indices arent enough, we want to store ranges if the id isnt lastID + 1
    // We should also keep free indices so that we never have to change the indices on any of our entities.

    
    // struct Grass {
    //     int positionIndex;
    //     int renderableIndex;
    //     int flammableIndex;

    //     public Grass() {
    //         positionIndex = positionSys.AddPosition();
    //         renderableIndex = renderableSys.AddRenderable('/');
    //         flammableIndex = flammableSys.AddFlammable();
    //     }
    // }
    // Do we even need this tho? Cant we just also have an empty grass which in flammable we'll just
    // loop up or something?
    // And maybe we dont even need that? maybe Grass is just an enum type of material?
    // Or is Grass a thing that has material?
    // What if we want a sword to have a material? (i guess in this case sword wouldnt be empty tho...)
    // Lets say given the sword we want to look up its material (sparse versus dense array): first I guess we'd
    // look up the range its in (tho these could be reallllly large) and then we'd have to search
    //    If we say that the dense matrix is stored with holes that would solve our problem in this case
    //    And going from material->sword would probably be fine because its very sparse
    
    // Elements and materials
    // Elements can change a materials state
    // Elements can change an elements state
    // Materials cant change other materials

    
    // Combine runes together to create spells
    // Many moves will have "activate" and "target"

    // @TODO: for rendering things we want to sort them all top left -> top right
    // We also want to efficiently find out what things to render and what to not
    // This will involve grouping things into cells and then finding out what cells
    //   we are rendering by doing box collision tests
    class Program {

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
        static int cameraSize = 20;
        static int renderableEntities = cameraSize * cameraSize;

        static void CollectRenderables() {
            
        }

        
        Renderable[] renderables = new Renderable[renderableEntities];

        static void RenderWorld() {
            int x = 0;
            for (int i = 0; i < renderableEntities; i++) {

                Console.Write(".");

                x++;

                if (x == cameraSize) {
                    x = 0;
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
        }
        
        static void Main(string[] args) {
            camera.box = new Box(cameraSize);
            
            GenerateWorld();

            while (true) {
                Console.Clear();

                RenderWorld();

                Console.ReadKey();
            }
            
            // Console.BackgroundColor = ConsoleColor.Blue;
            // Console.BackgroundColor = ConsoleColor.Red;
        }
    }
}
