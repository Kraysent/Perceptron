﻿using System;
using static System.Math;
using static System.Console;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetworkV2
{
    class Network
    {
        public int NumberOfInputs;
        public List<Neuron[]> Neurons;
        public double LearningRate;

        public int NumberOfLayers { get => Neurons.Count; }
        public int NumberOfOutputs { get => Neurons[NumberOfLayers - 1].Length; }

        /// <summary>
        /// Initialises empty neural network
        /// </summary>
        /// <param name="numberOfInputs"></param>
        public Network(int numberOfInputs, double learningRate = 0.1)
        {
            Neurons = new List<Neuron[]>();
            NumberOfInputs = numberOfInputs;
            LearningRate = learningRate;
        }

        /// <summary>
        /// Adds new layer of neurons to network
        /// </summary>
        /// <param name="numberOfNeurons"></param>
        /// <param name="activation"></param>
        /// <param name="activationDerivative"></param>
        public void AddLayer(int numberOfNeurons, Function activation, Function activationDerivative)
        {
            int i, currLayer = Neurons.Count;

            Neurons.Add(new Neuron[numberOfNeurons]);
            
            for (i = 0; i < numberOfNeurons; i++)
            {
                Neurons[currLayer][i] = new Neuron(activation, activationDerivative, NumberOfInputs);
            }
        }

        /// <summary>
        /// Network answer for single test
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public double[] ForwardPass(double[] inputs)
        {
            int i, j;
            double[] outputVector = new double[inputs.Length];

            for (i = 0; i < NumberOfLayers; i++)
            {
                outputVector = new double[Neurons[i].Length];

                for (j = 0; j < Neurons[i].Length; j++)
                {
                    outputVector[j] = Neurons[i][j].ForwardPass(inputs);
                }

                inputs = AddingOne(outputVector);
            }

            return outputVector;
        }

        /// <summary>
        /// Changes weight of each neuron and returns post-error for single example
        /// </summary>
        /// <param name="example"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public double TrainOnSingleExample(double[] example, double[] answer)
        {
            WriteLine("I - {0}; Ex - {1}", NumberOfInputs, example.Length);
            if (example.Length != NumberOfInputs) throw new Exception("Example vector has not the same length as number of inputs to network");
            if (answer.Length != NumberOfOutputs) throw new Exception("Answers vector has not the same length as number of outputs from network");

            //-------Forward pass with saved answers for each neuron-------//

            int i, j;
            double[] inputVector = example;
            List<double[]> outputs = new List<double[]>();

            for (i = 0; i < NumberOfLayers; i++)
            {
                outputs.Add(new double[Neurons[i].Length]);

                for (j = 0; j < Neurons[i].Length; j++)
                {
                    outputs[i][j] = Neurons[i][j].ForwardPass(inputVector);
                }

                inputVector = AddingOne(outputs[i]);
            }

            //-------Training-------//

            double[][] delta = new double[NumberOfOutputs][];
            int k;

            for (i = NumberOfLayers - 1; i >= 0; i--)
            {
                delta[i] = new double[Neurons[i].Length];

                for (j = 0; j < Neurons[i].Length; j++)
                {
                    /*
                     * delta = SigmaDerivative(summatory of current neuron) * 
                     *                                                          * ScalarProduct(Output weights of curr neuron, deltas of prev layer) for NOT output neurons
                     *                                                          or
                     *                                                          * (T - O) for output neurons, T - target answer, O - real answer
                     */
                    delta[i][j] = outputs[i][j] * (1 - outputs[i][j]) * ((i == NumberOfLayers - 1) ? (answer[j] - outputs[i][j]) : ScalarProduct(GetOutputWeights(i, j), delta[i + 1])); 

                    for (k = 0; k < Neurons[i][j].InputWeights.Length; k++)
                    {
                        Neurons[i][j].InputWeights[k] = Neurons[i][j].InputWeights[k] + delta[i][j] * LearningRate * ((i == 0) ? example[k] : outputs[i - 1][k]);
                    }
                }
            }

            //-------Counting error-------//

            double error = 0;
            double[] newAnswer = ForwardPass(example);

            for (i = 0; i < newAnswer.Length; i++)
            {
                error += Pow(newAnswer[i] - answer[i], 2);
            }

            return error;
        }

        /// <summary>
        /// Trains network on input matrix of data. Returns true, if network converged, false if not
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <param name="rightAnswers"></param>
        /// <param name="maxEpoches"></param>
        /// <param name="eps"></param>
        /// <returns></returns>
        public bool TrainUntilConvergence(double[][] inputMatrix, double[][] rightAnswers, int maxEpoches = (int)1e3, double eps = 1e-3)
        {
            if (inputMatrix.Length != rightAnswers.Length) throw new Exception("Number of example tests is not equal to number of right answers tests");

            int i, j;
            double prevErr = 0, currError = 0;

            for (i = 0; i < maxEpoches; i++)
            {
                for (j = 0; j < inputMatrix.Length; j++)
                {
                    currError = TrainOnSingleExample(inputMatrix[i], rightAnswers[i]);
                }

                if (Abs(currError - prevErr) < eps) return true;

                prevErr = currError;
            }

            return false;
        }

        /// <summary>
        /// Gets array of weights going out from current neuron
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public double[] GetOutputWeights(int layer, int number)
        {
            int i, j;
            List<double> output = new List<double>();

            if (layer == (Neurons.Count - 1)) throw new Exception("Output layer has no output weights");
            
            for (i = 0; i < Neurons[layer].Count(); i++)
            {
                for (j = 0; j < Neurons[layer][i].InputWeights.Length; j++)
                {
                    if (j == number)
                    {
                        output.Add(Neurons[layer][i].InputWeights[j]);
                    }
                }
            }

            return output.ToArray();
        }

        private static double[] AddingOne(double[] input)
        {
            List<double> arr = input.ToList();

            arr.Insert(0, 1);

            return arr.ToArray();
        }

        private static double ScalarProduct(double[] vector1, double[] vector2)
        {
            int i;
            double sum = 0;

            if (vector1.Length != vector2.Length) throw new Exception("Vectors have not the same length");

            for (i = 0; i < vector1.Length; i++)
            {
                sum += vector2[i] * vector1[i];
            }

            return sum;
        }
    }
}
