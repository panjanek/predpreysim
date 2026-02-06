# Predator Prey simulation with evolution
Predator-Prey model with agents controlled by small (196 weights) neural networks. 
Networks evolve by mutation and crossing over of the fittest.
Mechanics implemented in GPU shaders. Evolution done on CPU side.

## Mechanics
 * Plants, prey and predators emit a scent
 * Plants are stationary, regrow at different, random, place after eaten
 * Scents map, consisting of RGB values (G=plant, B=prey, R=pred), precomputed on GPU (blurring)
 * Each agent sense environment with
   * 5 sensors per each scent - 15 scent sensors
   * 2 differential sensors - difference or forward directed sensors
 * Each agent has 2 memory cells (registers)
 * Each agent controlled by neural network:
   * 19 inputs (15 absolute sensors, 2 differential sensors, 2 memory registers)
   * 12 hidden layers
   * 4 outputs
     * angular velocity
     * forward velocity
     * change to memory registers
   * tanh activation function
   * each nauron has one bias
   * 196 float parameters per agent. This is its genom

## Evolution
 * Start with randomly initialized neural networks
 * Each generation (5000 - 30000 simulation steps) perform selection
 * Select top 10% performers by fitness function
   * eating a meal awarded
   * being eated punished
   * spending energy punished (energy expenditure = velocity squared)
   * fitness decay with age to avoid immortal ellites
   * prefer diverse mating pool (preference toward agents that differ from each other. Difference computed as a "behavioral distance" - difference of network responses for given fixed inputs)
 * Choose best performers to mate using beta distribution (better mate more often)
 * Mutation
   * Change randomly selected weights by a gausian random value
   * Change biases with half magnitute
   * Change weights affecting memory with 1/3 magnitude
 * Crossing over
   * Take neurons from parents randomly
   * Take layers from parents randomly
 * replace bottom 50% performers with newly breeded agents

## Behavior
 * Random chaotic movements on start
 * Prey start to be attracted to green and eat plants (prey fitness rises)
 * Predators learn to chase prey (predator fitness raises, prey struggle)
 * Prey learn to escape predators (predator fitness decreases, prey rises)
 * After 100 - 300 generations: stabilization. Both species evolve sensible behaviors

# Visualisations

| <p align="center"><br/></p> | <p align="center"><img src="https://github.com/panjanek/predpreysim/blob/3054890fd0b513ed57aadf631a57c93fc96a3d2c/shots/chase.gif" /><br/>after 50 generations: chase</p> | <p align="center"><br/></p> |
|---|---|---|

<p align="center"><img src="https://github.com/panjanek/predpreysim/blob/3054890fd0b513ed57aadf631a57c93fc96a3d2c/shots/screen.png" /></p>
