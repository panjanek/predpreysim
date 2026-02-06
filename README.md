# Predator Prey simulation with evolution
Predator-Prey model with agents controlled by small (196 weights) neural networks. Networks evolve by mutation and crossing over of the fittest.
Mechanics implemented GPU shaders. Evolution done on CPU side.

## Mechanics
 * Plants, prey and predators emit a scent
 * Plants are stationary, regrow at different, random, place when eaten
 * Scents map, consisting of RGB values (G=plant, B=prey, R=pred) precomputed on GPU (blurring)
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
   
