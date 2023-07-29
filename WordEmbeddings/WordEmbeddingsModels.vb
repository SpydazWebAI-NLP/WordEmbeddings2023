﻿Imports System.Drawing
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports WordEmbeddings.Word2Vectors.Factory
Imports WordEmbeddings.Word2Vectors.Models
Namespace Word2Vectors

    Namespace Factory
        Public MustInherit Class WordEmbeddingsModel

            ' A simple vocabulary for demonstration purposes.
            Private iVocabulary As New List(Of String) From {"apple", "orange", "banana", "grape", "cherry"}
            Public Property Vocabulary As List(Of String)
                Get
                    Return iVocabulary
                End Get
                Set(value As List(Of String))
                    iVocabulary = value
                End Set
            End Property
            ' Word embeddings dictionary to store the learned word vectors.
            Public WordEmbeddings As New WordEmbedding

            ' Hyperparameters for training.
            Public EmbeddingSize As Integer = 50 ' Size of word vectors.
            Public WindowSize As Integer = 2 ' Context window size.

            Public LearningRate As Double = 0.01 ' Learning rate for gradient descent.
            Public NumEpochs As Integer = 1000 ' Number of training epochs.

            ' Random number generator for initialization.
            Public Shared ReadOnly Rand As New Random()
            Public MustOverride Sub Train()
            Public MustOverride Sub Train(corpus As List(Of List(Of String)))
            Public Sub New(ByRef model As WordEmbeddingsModel)
                iVocabulary = model.Vocabulary
                WordEmbeddings = model.WordEmbeddings
                EmbeddingSize = model.EmbeddingSize
                WindowSize = model.WindowSize
                LearningRate = model.LearningRate
                NumEpochs = model.NumEpochs
            End Sub
            Public Sub New(ByRef Vocabulary As List(Of String))
                iVocabulary = Vocabulary
            End Sub
            Public Function ExportModel() As WordEmbeddingsModel
                Return Me
            End Function
            Public Sub SetTrainingParameters(ByRef Embeddingsize As Integer,
                                         ByRef WindowSize As Integer,
                                         ByRef LearningRate As Double, ByRef Epochs As Integer)
                Me.EmbeddingSize = Embeddingsize
                Me.WindowSize = WindowSize
                Me.LearningRate = LearningRate
                Me.NumEpochs = Epochs
            End Sub
            ' WordEmbedding class to store word vectors and handle operations on word embeddings.
            Public Class WordEmbedding
                Public embeddings As Dictionary(Of String, Double())

                Public Sub New()
                    Me.embeddings = New Dictionary(Of String, Double())()
                End Sub

                Public Sub Add(word As String, vector As Double())
                    embeddings(word) = vector
                End Sub

                Public Function GetVector(word As String) As Double()
                    Return embeddings(word)
                End Function

                ' Implement other operations as needed for word embeddings.
                ' E.g., similarity, word lookup, etc.
            End Class
            Public Function ComputeDotProduct(vec1 As Double(), vec2 As Double()) As Double
                Return Enumerable.Range(0, EmbeddingSize).Sum(Function(i) vec1(i) * vec2(i))
            End Function

            Public Function Sigmoid(x As Double) As Double
                Return 1.0 / (1.0 + Math.Exp(-x))
            End Function

            ''' <summary>
            ''' Cosine Similarity(A, B) = (A dot B) / (||A|| * ||B||)
            '''  where:
            '''  A And B are the word vectors of two words.
            '''  A dot B Is the dot product Of the two vectors.
            '''  ||A|| And ||B|| are the magnitudes (Euclidean norms) of the vectors.
            '''  The cosine similarity ranges from -1 To 1, where 1 indicates the highest similarity, 0 indicates no similarity, And -1 indicates the highest dissimilarity.
            ''' </summary>
            ''' <param name="word1"></param>
            ''' <param name="word2"></param>
            ''' <param name="wordEmbeddings"></param>
            ''' <returns></returns>
            Public Function CosineSimilarity(word1 As String, word2 As String, wordEmbeddings As WordEmbedding) As Double
                Dim vector1 As Double() = wordEmbeddings.GetVector(word1)
                Dim vector2 As Double() = wordEmbeddings.GetVector(word2)

                ' Calculate the dot product of the two vectors.
                Dim dotProduct As Double = 0
                For i As Integer = 0 To vector1.Length - 1
                    dotProduct += vector1(i) * vector2(i)
                Next

                ' Calculate the magnitudes of the vectors.
                Dim magnitude1 As Double = Math.Sqrt(vector1.Sum(Function(x) x * x))
                Dim magnitude2 As Double = Math.Sqrt(vector2.Sum(Function(x) x * x))

                ' Calculate the cosine similarity.
                Dim similarity As Double = dotProduct / (magnitude1 * magnitude2)

                Return similarity
            End Function
            Public Function DisplayMatrix(matrix As Dictionary(Of String, Dictionary(Of String, Double))) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Object)
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Object = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function
            Public Sub DisplayEmbeddingsModel()
                Dim dgv = DisplayMatrix(WordEmbeddings.embeddings)

                ' Create a form and add the DataGridView to it
                Dim kform As New Form
                kform.Text = "Embedding Matrix"
                kform.Size = New Size(800, 600)
                kform.Controls.Add(dgv)

                ' Display the form
                Application.Run(kform)
            End Sub
            Public Function DisplayMatrix(matrix As Dictionary(Of String, Double())) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Object)()
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Integer = If(matrix.ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function
            Public Function DisplayMatrix(matrix As Dictionary(Of String, Dictionary(Of String, Integer))) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Integer)
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Object = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function


        End Class

        ''' <summary>
        ''' One possible way to combine the approaches is by using a two-step training process:
        '''  Pre-training Using Skip-gram With Negative Sampling:
        '''   In this step, 
        '''    you can pre-train the word embeddings using the skip-gram model 
        '''    with negative sampling on a large dataset Or a diverse corpus. 
        '''    This step allows you to efficiently learn word embeddings 
        '''    in a computationally efficient 
        '''    manner while capturing semantic relationships between words.
        '''  Fine-tuning using Hierarchical Softmax:
        '''   After pre-training the word embeddings, 
        '''    you can perform fine-tuning Using the hierarchical softmax technique. 
        '''    During the fine-tuning Step, 
        '''    you can use a smaller dataset 
        '''   Or a more domain-specific corpus 
        '''    To train the model Using hierarchical softmax. 
        '''    This Step enables you To refine the word embeddings 
        '''    And make them more accurate And context-specific.
        ''' </summary>
        Public Class HybridWordEmbeddingsModel
            Inherits WordEmbeddingsModel

            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub

            Public Sub New(ByRef Vocabulary As List(Of String))
                MyBase.New(Vocabulary)
            End Sub
            Public Enum ModelType
                Skipgram
                Glove
                SoftMax
                CBOW
                FastText
            End Enum
            Public Function PreTrain(ByRef model As WordEmbeddingsModel, ByRef iModelType As ModelType) As WordEmbeddingsModel
                model.Train()
                Dim preTrainedModel As WordEmbeddingsModel


                Select Case iModelType
                    Case ModelType.Skipgram
                        preTrainedModel = New WordEmbeddingsWithNegativeSampling(model.Vocabulary)
                        preTrainedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs)  ' Set appropriate parameters for pre-training

                        preTrainedModel.Train() ' Pre-train the word embeddings using Skip-gram with Negative Sampling

                        Return preTrainedModel
                    Case ModelType.Glove
                        preTrainedModel = New WordEmbeddingsWithGloVe(model.Vocabulary)
                        preTrainedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs)  ' Set appropriate parameters for pre-training

                        preTrainedModel.Train() ' Pre-train the word embeddings using Skip-gram with Negative Sampling

                        Return preTrainedModel
                    Case ModelType.FastText
                        preTrainedModel = New WordEmbeddingsWithFastText(model.Vocabulary)
                        preTrainedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs)  ' Set appropriate parameters for pre-training

                        preTrainedModel.Train() ' Pre-train the word embeddings using Skip-gram with Negative Sampling

                        Return preTrainedModel
                    Case ModelType.CBOW
                        preTrainedModel = New WordEmbeddingsWithCBOW(model.Vocabulary)
                        preTrainedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs)  ' Set appropriate parameters for pre-training

                        preTrainedModel.Train() ' Pre-train the word embeddings using Skip-gram with Negative Sampling

                        Return preTrainedModel
                    Case ModelType.SoftMax
                        preTrainedModel = New WordEmbeddingsWithHierarchicalSoftmax(model.Vocabulary)
                        preTrainedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs)  ' Set appropriate parameters for pre-training

                        preTrainedModel.Train() ' Pre-train the word embeddings using Skip-gram with Negative Sampling

                        Return preTrainedModel
                End Select
                Return model
            End Function

            Public Function FineTune(ByRef Model As WordEmbeddingsModel, ByRef iModelType As ModelType) As WordEmbeddingsModel

                Dim fineTunedModel As WordEmbeddingsModel

                Model.Train()

                Select Case iModelType
                    Case ModelType.CBOW
                        fineTunedModel = New WordEmbeddingsWithCBOW(Model.Vocabulary)
                        fineTunedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for fine-tuning
                        fineTunedModel.Train() ' Fine-tune the word embeddings using Hierarchical Softmax
                        Return fineTunedModel
                    Case ModelType.FastText
                        fineTunedModel = New WordEmbeddingsWithFastText(Model.Vocabulary)
                        fineTunedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for fine-tuning
                        fineTunedModel.Train() ' Fine-tune the word embeddings using Hierarchical Softmax
                        Return fineTunedModel
                    Case ModelType.Glove
                        fineTunedModel = New WordEmbeddingsWithGloVe(Model.Vocabulary)
                        fineTunedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for fine-tuning
                        fineTunedModel.Train() ' Fine-tune the word embeddings using Hierarchical Softmax
                        Return fineTunedModel
                    Case ModelType.Skipgram
                        fineTunedModel = New WordEmbeddingsWithNegativeSampling(Model.Vocabulary)
                        fineTunedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for fine-tuning
                        fineTunedModel.Train() ' Fine-tune the word embeddings using Hierarchical Softmax
                        Return fineTunedModel
                    Case ModelType.SoftMax
                        fineTunedModel = New WordEmbeddingsWithHierarchicalSoftmax(Model.Vocabulary)
                        fineTunedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for fine-tuning
                        fineTunedModel.Train() ' Fine-tune the word embeddings using Hierarchical Softmax
                        Return fineTunedModel

                End Select


                Return Model

            End Function


            Public Overloads Sub Train(Optional PretrainModel As ModelType = ModelType.Skipgram, Optional FineTuneModel As ModelType = ModelType.Glove)
                Dim hybrid As New HybridWordEmbeddingsModel(Vocabulary)
                Dim preTrainedModel = PreTrain(hybrid, PretrainModel)
                Dim fineTunedModel = FineTune(preTrainedModel, FineTuneModel)
                'set model
                Me.WordEmbeddings = fineTunedModel.WordEmbeddings

            End Sub


            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                ' Step 1: Pre-training using Skip-gram with Negative Sampling.
                Console.WriteLine("Pre-training using Skip-gram with Negative Sampling...")
                Dim preTrainedModel As New WordEmbeddingsWithNegativeSampling(Vocabulary)
                preTrainedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for pre-training
                preTrainedModel.Train(corpus) ' Pre-train the word embeddings using Skip-gram with Negative Sampling


                ' Step 3: Fine-tuning using Hierarchical Softmax.
                Console.WriteLine("Fine-tuning using Hierarchical Softmax...")
                Dim fineTunedModel As New WordEmbeddingsWithHierarchicalSoftmax(Vocabulary)
                fineTunedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for fine-tuning
                fineTunedModel.Train(corpus) ' Fine-tune the word embeddings using Hierarchical Softmax

                ' Step 4: Set the fine-tuned word embeddings as the model's word embeddings.
                WordEmbeddings = fineTunedModel.WordEmbeddings

                Console.WriteLine("Training completed!")
            End Sub

            Public Overrides Sub Train()
                ' Step 1: Pre-training using Skip-gram with Negative Sampling.
                Console.WriteLine("Pre-training using Skip-gram with Negative Sampling...")
                Dim preTrainedModel As New WordEmbeddingsWithNegativeSampling(Vocabulary)
                preTrainedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for pre-training
                preTrainedModel.train() ' Pre-train the word embeddings using Skip-gram with Negative Sampling


                ' Step 3: Fine-tuning using Hierarchical Softmax.
                Console.WriteLine("Fine-tuning using Hierarchical Softmax...")
                Dim fineTunedModel As New WordEmbeddingsWithHierarchicalSoftmax(Vocabulary)
                fineTunedModel.SetTrainingParameters(EmbeddingSize, WindowSize, LearningRate, NumEpochs) ' Set appropriate parameters for fine-tuning
                fineTunedModel.Train() ' Fine-tune the word embeddings using Hierarchical Softmax

                ' Step 4: Set the fine-tuned word embeddings as the model's word embeddings.
                WordEmbeddings = fineTunedModel.WordEmbeddings

                Console.WriteLine("Training completed!")
            End Sub


        End Class

    End Namespace
    Namespace Models

        ''' <summary>
        '''Skip-gram with Negative Sampling:
        ''' Pros:
        ''' More computationally efficient: Negative sampling reduces the computational cost by Using a small number Of negative samples For Each positive context pair during training.
        ''' Simpler to implement: It's relatively easier to implement skip-gram with negative sampling compared to hierarchical softmax.
        ''' Performs well With large vocabularies: Negative sampling Is well-suited For training word embeddings With large vocabularies As it scales well.
        ''' Cons:
        ''' May sacrifice quality: With negative sampling, some negative samples may Not be truly informative, potentially leading To a slight degradation In the quality Of learned word embeddings compared To hierarchical softmax.
        ''' Tuning hyperparameters: The effectiveness Of negative sampling depends On the selection Of the number Of negative samples And learning rate, which may require tuning. 
        ''' </summary>
        Public Class WordEmbeddingsWithNegativeSampling
            Inherits WordEmbeddingsModel
            Public NumNegativeSamples As Integer = 5 ' Number of negative samples per positive sample.

            Public Sub New(ByRef Vocabulary As List(Of String), Optional NumberOfNegativeSamples As Integer = 5)
                MyBase.New(Vocabulary)
                Me.NumNegativeSamples = NumberOfNegativeSamples
            End Sub
            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub
            Public Overrides Sub train()
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Simulate training data (context pairs).
                Dim trainingData As New List(Of (String, String))()
                For i As Integer = 0 To Vocabulary.Count - 1
                    For j As Integer = Math.Max(0, i - WindowSize) To Math.Min(Vocabulary.Count - 1, i + WindowSize)
                        If i <> j Then
                            trainingData.Add((Vocabulary(i), Vocabulary(j)))
                        End If
                    Next
                Next

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    trainingData = trainingData.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each context pair.
                    For Each item In trainingData
                        ' Generate negative samples.
                        Dim negativeSamples As New List(Of String)()
                        While negativeSamples.Count < NumNegativeSamples
                            Dim randomWord = Vocabulary(Rand.Next(Vocabulary.Count))
                            If randomWord <> item.Item1 AndAlso randomWord <> item.Item2 AndAlso Not negativeSamples.Contains(randomWord) Then
                                negativeSamples.Add(randomWord)
                            End If
                        End While

                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2, negativeSamples)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub

            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    Dim vector As Double() = Enumerable.Range(0, EmbeddingSize).Select(Function(i) Rand.NextDouble()).ToArray()
                    WordEmbeddings.Add(word, vector)
                Next

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    For Each document In corpus
                        For wordIndex As Integer = 0 To document.Count - 1
                            Dim targetWord As String = document(wordIndex)
                            Dim contextStart As Integer = Math.Max(0, wordIndex - WindowSize)
                            Dim contextEnd As Integer = Math.Min(document.Count - 1, wordIndex + WindowSize)

                            ' Skip-gram with negative sampling.
                            For contextIndex As Integer = contextStart To contextEnd
                                If contextIndex = wordIndex Then Continue For ' Skip the target word itself.

                                Dim contextWord As String = document(contextIndex)
                                Dim loss As Double = 0

                                ' Positive pair (target word, context word).
                                Dim targetVector As Double() = WordEmbeddings.GetVector(targetWord)
                                Dim contextVector As Double() = WordEmbeddings.GetVector(contextWord)
                                Dim dotProduct As Double = ComputeDotProduct(targetVector, contextVector)
                                Dim sigmoidDotProduct As Double = Sigmoid(dotProduct)
                                loss += -Math.Log(sigmoidDotProduct)

                                ' Negative sampling (sample k negative words).
                                Dim numNegativeSamples As Integer = 5
                                For i As Integer = 1 To numNegativeSamples
                                    Dim negativeWord As String = Vocabulary(Rand.Next(Vocabulary.Count))
                                    If negativeWord = targetWord OrElse negativeWord = contextWord Then Continue For ' Skip positive pairs.

                                    Dim negativeVector As Double() = WordEmbeddings.GetVector(negativeWord)
                                    Dim negDotProduct As Double = ComputeDotProduct(targetVector, negativeVector)
                                    Dim sigmoidNegDotProduct As Double = Sigmoid(negDotProduct)
                                    loss += -Math.Log(1 - sigmoidNegDotProduct)
                                Next

                                ' Update word vectors using gradient descent.
                                Dim learningRateFactor As Double = LearningRate * (1 - (epoch / NumEpochs)) ' Reduce learning rate over epochs.
                                Dim gradient As Double = sigmoidDotProduct - 1 ' Gradient for positive pair.
                                For i As Integer = 0 To EmbeddingSize - 1
                                    targetVector(i) -= learningRateFactor * gradient * contextVector(i)
                                    contextVector(i) -= learningRateFactor * gradient * targetVector(i)
                                Next

                                ' Update word vectors for negative samples.
                                For i As Integer = 1 To numNegativeSamples
                                    Dim negativeWord As String = Vocabulary(Rand.Next(Vocabulary.Count))
                                    If negativeWord = targetWord OrElse negativeWord = contextWord Then Continue For ' Skip positive pairs.

                                    Dim negativeVector As Double() = WordEmbeddings.GetVector(negativeWord)
                                    Dim negDotProduct As Double = ComputeDotProduct(targetVector, negativeVector)
                                    Dim sigmoidNegDotProduct As Double = Sigmoid(negDotProduct)
                                    Dim negGradient As Double = sigmoidNegDotProduct ' Gradient for negative pair.

                                    For j As Integer = 0 To EmbeddingSize - 1
                                        targetVector(j) -= learningRateFactor * negGradient * negativeVector(j)
                                        negativeVector(j) -= learningRateFactor * negGradient * targetVector(j)
                                    Next
                                Next

                                ' Update the embeddings for target and context words.
                                WordEmbeddings.Add(targetWord, targetVector)
                                WordEmbeddings.Add(contextWord, contextVector)
                            Next
                        Next
                    Next
                Next
            End Sub

            Private Sub Update(targetWord As String, contextWord As String, negativeSamples As List(Of String))
                Dim targetEmbedding = WordEmbeddings.GetVector(targetWord)
                Dim contextEmbedding = WordEmbeddings.GetVector(contextWord)

                Dim targetLoss As Double = 0
                Dim contextLoss As Double = 0

                ' Compute the loss for the positive context pair.
                Dim positiveScore As Double = ComputeDotProduct(targetEmbedding, contextEmbedding)
                Dim positiveSigmoid As Double = Sigmoid(positiveScore)
                targetLoss += -Math.Log(positiveSigmoid)
                contextLoss += -Math.Log(positiveSigmoid)

                ' Compute the loss for the negative samples.
                For Each negativeWord In negativeSamples
                    Dim negativeEmbedding = WordEmbeddings.GetVector(negativeWord)
                    Dim negativeScore As Double = ComputeDotProduct(targetEmbedding, negativeEmbedding)
                    Dim negativeSigmoid As Double = Sigmoid(negativeScore)
                    targetLoss += -Math.Log(1 - negativeSigmoid)
                Next

                ' Compute the gradients and update the word embeddings.
                Dim targetGradient = contextEmbedding.Clone()
                Dim contextGradient = targetEmbedding.Clone()

                targetGradient = targetGradient.Select(Function(g) g * (positiveSigmoid - 1)).ToArray()
                contextGradient = contextGradient.Select(Function(g) g * (positiveSigmoid - 1)).ToArray()

                For Each negativeWord In negativeSamples
                    Dim negativeEmbedding = WordEmbeddings.GetVector(negativeWord)
                    Dim negativeSigmoid As Double = Sigmoid(ComputeDotProduct(targetEmbedding, negativeEmbedding))

                    For i As Integer = 0 To EmbeddingSize - 1
                        targetGradient(i) += negativeSigmoid * negativeEmbedding(i)
                        negativeEmbedding(i) += negativeSigmoid * targetEmbedding(i)
                    Next
                Next

                ' Update the word embeddings using the computed gradients.
                For i As Integer = 0 To EmbeddingSize - 1
                    targetEmbedding(i) -= LearningRate * targetGradient(i)
                    contextEmbedding(i) -= LearningRate * contextGradient(i)
                Next
            End Sub



        End Class
        ''' <summary>
        '''Hierarchical Softmax
        ''' Pros:
        ''' Theoretically more accurate: Hierarchical softmax provides more accurate training by transforming the softmax operation into a binary tree-based probability calculation, ensuring that Each word Is considered during training.
        ''' Better performance With smaller datasets: Hierarchical softmax Is more suitable For smaller datasets, where negative sampling might Not perform As well due To a limited number Of contexts.
        ''' Cons:
        ''' Computationally expensive For large vocabularies: Hierarchical softmax can become computationally expensive With larger vocabularies, As it requires traversing a binary tree To compute probabilities For Each word during training.
        ''' More complex To implement: Implementing hierarchical softmax can be more complex compared To negative sampling.
        ''' </summary>
        Public Class WordEmbeddingsWithHierarchicalSoftmax
            Inherits WordEmbeddingsModel
            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub
            Public Sub New(ByRef Vocabulary As List(Of String))
                MyBase.New(Vocabulary)
            End Sub

            Public Overrides Sub Train()
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Simulate training data (context pairs).
                Dim trainingData As New List(Of (String, String))()
                For i As Integer = 0 To Vocabulary.Count - 1
                    For j As Integer = Math.Max(0, i - WindowSize) To Math.Min(Vocabulary.Count - 1, i + WindowSize)
                        If i <> j Then
                            trainingData.Add((Vocabulary(i), Vocabulary(j)))
                        End If
                    Next
                Next

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    trainingData = trainingData.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each context pair.
                    For Each item In trainingData
                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub
            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Build the hierarchical softmax binary tree.
                Dim rootNode As New Node(Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                For Each word In Vocabulary
                    Dim pathToWord As List(Of Node) = GetPathToWord(rootNode, word)
                    Dim currentNode As Node = rootNode
                    For Each node In pathToWord
                        If node Is Nothing Then
                            Dim newNode As New Node(Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                            newNode.Parent = currentNode
                            If currentNode.Left Is Nothing Then
                                currentNode.Left = newNode
                            Else
                                currentNode.Right = newNode
                            End If
                            currentNode = newNode
                        Else
                            currentNode = node
                        End If
                    Next
                    currentNode.Word = word
                Next

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    Dim trainingData As New List(Of (String, String))()
                    For Each document In corpus
                        For wordIndex As Integer = 0 To document.Count - 1
                            Dim targetWord As String = document(wordIndex)
                            Dim contextStart As Integer = Math.Max(0, wordIndex - WindowSize)
                            Dim contextEnd As Integer = Math.Min(document.Count - 1, wordIndex + WindowSize)

                            For contextIndex As Integer = contextStart To contextEnd
                                If contextIndex = wordIndex Then Continue For ' Skip the target word itself.

                                trainingData.Add((targetWord, document(contextIndex)))
                            Next
                        Next
                    Next

                    ' Shuffle the training data.
                    trainingData = trainingData.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each context pair.
                    For Each item In trainingData
                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2, rootNode)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub

            Private Sub Update(targetWord As String, contextWord As String, rootNode As Node)
                Dim targetEmbedding = WordEmbeddings.GetVector(targetWord)
                Dim contextEmbedding = WordEmbeddings.GetVector(contextWord)

                Dim pathToContext = GetPathToWord(rootNode, contextWord)

                Dim targetLoss As Double = 0
                Dim contextLoss As Double = 0

                ' Compute the loss for the positive context pair.
                Dim positiveScore As Double = 0
                For Each node In pathToContext
                    positiveScore += ComputeDotProduct(targetEmbedding, node.Vector)
                Next
                Dim positiveSigmoid As Double = Sigmoid(positiveScore)
                targetLoss += -Math.Log(positiveSigmoid)
                contextLoss += -Math.Log(positiveSigmoid)

                ' Update the gradients for the target word.
                For Each node In pathToContext
                    Dim sigmoidGradient As Double = (positiveSigmoid - 1.0) * LearningRate

                    For i As Integer = 0 To EmbeddingSize - 1
                        node.Vector(i) -= sigmoidGradient * targetEmbedding(i)
                        targetEmbedding(i) -= sigmoidGradient * node.Vector(i)
                    Next

                    ' Move to the parent node.
                    node = node.Parent
                Next

                ' Update the gradients for the context word.
                For Each node In pathToContext
                    Dim sigmoidGradient As Double = (positiveSigmoid - 1.0) * LearningRate

                    For i As Integer = 0 To EmbeddingSize - 1
                        node.Vector(i) -= sigmoidGradient * contextEmbedding(i)
                        contextEmbedding(i) -= sigmoidGradient * node.Vector(i)
                    Next
                Next
            End Sub

            Private Function GetPathToWord(startNode As Node, word As String) As List(Of Node)
                Dim path As New List(Of Node)()
                Dim currentNode As Node = startNode

                While currentNode IsNot Nothing
                    path.Add(currentNode)
                    If currentNode.Word = word Then
                        Exit While
                    ElseIf WordEmbeddings.GetVector(word)(0) < 0 Then
                        currentNode = currentNode.Left
                    Else
                        currentNode = currentNode.Right
                    End If
                End While

                Return path
            End Function

            Private Sub Update(targetWord As String, contextWord As String)
                Dim targetEmbedding = WordEmbeddings.GetVector(targetWord)
                Dim contextEmbedding = WordEmbeddings.GetVector(contextWord)

                Dim pathToTarget = GetPathToWord(targetWord)
                Dim pathToContext = GetPathToWord(contextWord)

                Dim targetLoss As Double = 0
                Dim contextLoss As Double = 0

                ' Compute the loss for the positive context pair.
                Dim positiveScore As Double = 0
                For Each node In pathToContext
                    positiveScore += ComputeDotProduct(targetEmbedding, node.Vector)
                Next
                Dim positiveSigmoid As Double = Sigmoid(positiveScore)
                targetLoss += -Math.Log(positiveSigmoid)
                contextLoss += -Math.Log(positiveSigmoid)

                ' Update the gradients for the target word.
                For Each node In pathToTarget
                    Dim sigmoidGradient As Double = (positiveSigmoid - 1.0) * LearningRate

                    For i As Integer = 0 To EmbeddingSize - 1
                        node.Vector(i) -= sigmoidGradient * contextEmbedding(i)
                        contextEmbedding(i) -= sigmoidGradient * node.Vector(i)
                    Next

                    ' Move to the parent node.
                    node = node.Parent
                Next

                ' Update the gradients for the context word.
                For Each node In pathToContext
                    Dim sigmoidGradient As Double = (positiveSigmoid - 1.0) * LearningRate

                    For i As Integer = 0 To EmbeddingSize - 1
                        node.Vector(i) -= sigmoidGradient * targetEmbedding(i)
                        targetEmbedding(i) -= sigmoidGradient * node.Vector(i)
                    Next
                Next
            End Sub

            Private Function GetPathToWord(word As String) As List(Of Node)
                Dim path As New List(Of Node)()
                Dim currentNode As Node = New Node(WordEmbeddings.GetVector(word))

                While currentNode IsNot Nothing
                    path.Add(currentNode)
                    currentNode = currentNode.Parent
                End While

                Return path
            End Function



            ' Helper class to represent nodes in the hierarchical softmax binary tree.
            Private Class Node
                Public Property Vector As Double()
                Public Property Left As Node
                Public Property Right As Node
                Public Property Parent As Node
                Public Property Word As String
                Public Sub New(vector As Double())
                    Me.Vector = vector
                End Sub
            End Class

        End Class
        Public Class WordEmbeddingsWithGloVe
            Inherits WordEmbeddingsModel

            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub

            Public Sub New(ByRef Vocabulary As List(Of String))
                MyBase.New(Vocabulary)
            End Sub

            Public Overrides Sub Train()
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Construct the global word co-occurrence matrix.
                Dim coOccurrenceMatrix = BuildCoOccurrenceMatrix()

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    coOccurrenceMatrix = coOccurrenceMatrix.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each word pair in the co-occurrence matrix.
                    For Each item In coOccurrenceMatrix
                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2, item.Item3)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub
            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Construct the global word co-occurrence matrix.
                Dim coOccurrenceMatrix = BuildCoOccurrenceMatrix(corpus)

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    coOccurrenceMatrix = coOccurrenceMatrix.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each word pair in the co-occurrence matrix.
                    For Each item In coOccurrenceMatrix
                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2, item.Item3)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub

            Private Function BuildCoOccurrenceMatrix(corpus As List(Of List(Of String))) As List(Of (String, String, Double))
                ' Construct a global word co-occurrence matrix.
                Dim coOccurrenceMatrix As New List(Of (String, String, Double))()

                ' Simulate training data (context pairs).
                For Each document In corpus
                    For wordIndex As Integer = 0 To document.Count - 1
                        Dim targetWord As String = document(wordIndex)
                        Dim contextStart As Integer = Math.Max(0, wordIndex - WindowSize)
                        Dim contextEnd As Integer = Math.Min(document.Count - 1, wordIndex + WindowSize)

                        For contextIndex As Integer = contextStart To contextEnd
                            If contextIndex = wordIndex Then Continue For ' Skip the target word itself.

                            ' Increment the co-occurrence count for the word pair (targetWord, contextWord).
                            Dim coOccurrenceCount = 1.0 / (Math.Abs(contextIndex - wordIndex))
                            coOccurrenceMatrix.Add((targetWord, document(contextIndex), coOccurrenceCount))
                        Next
                    Next
                Next

                Return coOccurrenceMatrix
            End Function
            Private Function BuildCoOccurrenceMatrix() As List(Of (String, String, Double))
                ' Construct a global word co-occurrence matrix.
                Dim coOccurrenceMatrix As New List(Of (String, String, Double))()

                ' Simulate training data (context pairs).
                For i As Integer = 0 To Vocabulary.Count - 1
                    For j As Integer = Math.Max(0, i - WindowSize) To Math.Min(Vocabulary.Count - 1, i + WindowSize)
                        If i <> j Then
                            ' Increment the co-occurrence count for the word pair (Vocabulary(i), Vocabulary(j)).
                            Dim coOccurrenceCount = 1.0 / (Math.Abs(i - j))
                            coOccurrenceMatrix.Add((Vocabulary(i), Vocabulary(j), coOccurrenceCount))
                        End If
                    Next
                Next

                Return coOccurrenceMatrix
            End Function

            Private Sub Update(word1 As String, word2 As String, coOccurrenceCount As Double)
                Dim vector1 = WordEmbeddings.GetVector(word1)
                Dim vector2 = WordEmbeddings.GetVector(word2)

                Dim dotProduct As Double = ComputeDotProduct(vector1, vector2)
                Dim loss As Double = (dotProduct - Math.Log(coOccurrenceCount)) ^ 2

                Dim gradient1 = New Double(EmbeddingSize - 1) {}
                Dim gradient2 = New Double(EmbeddingSize - 1) {}

                For i As Integer = 0 To EmbeddingSize - 1
                    gradient1(i) = 2.0 * (dotProduct - Math.Log(coOccurrenceCount)) * vector2(i)
                    gradient2(i) = 2.0 * (dotProduct - Math.Log(coOccurrenceCount)) * vector1(i)
                Next

                ' Update the word embeddings using the computed gradients.
                For i As Integer = 0 To EmbeddingSize - 1
                    vector1(i) -= LearningRate * gradient1(i)
                    vector2(i) -= LearningRate * gradient2(i)
                Next
            End Sub

        End Class
        Public Class WordEmbeddingsWithFastText
            Inherits WordEmbeddingsModel

            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub

            Public Sub New(ByRef vocabulary As List(Of String))
                MyBase.New(vocabulary)
            End Sub

            Public Overrides Sub Train()
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Simulate training data (context pairs).
                Dim trainingData As New List(Of (String, String))()
                For i As Integer = 0 To Vocabulary.Count - 1
                    For j As Integer = Math.Max(0, i - WindowSize) To Math.Min(Vocabulary.Count - 1, i + WindowSize)
                        If i <> j Then
                            trainingData.Add((Vocabulary(i), Vocabulary(j)))
                        End If
                    Next
                Next

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    trainingData = trainingData.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each context pair.
                    For Each item In trainingData
                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub
            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Simulate training data (context pairs).
                Dim trainingData As New List(Of (String, String))()
                For i As Integer = 0 To Vocabulary.Count - 1
                    For j As Integer = Math.Max(0, i - WindowSize) To Math.Min(Vocabulary.Count - 1, i + WindowSize)
                        If i <> j Then
                            trainingData.Add((Vocabulary(i), Vocabulary(j)))
                        End If
                    Next
                Next

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    trainingData = trainingData.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each context pair.
                    For Each item In trainingData
                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub

            Private Sub Update(targetWord As String, contextWord As String)
                Dim targetEmbedding = WordEmbeddings.GetVector(targetWord)
                Dim contextEmbedding = WordEmbeddings.GetVector(contextWord)

                Dim targetLoss As Double = 0
                Dim contextLoss As Double = 0

                ' Compute the loss for the positive context pair.
                Dim positiveScore As Double = ComputeDotProduct(targetEmbedding, contextEmbedding)
                Dim positiveSigmoid As Double = Sigmoid(positiveScore)
                targetLoss += -Math.Log(positiveSigmoid)
                contextLoss += -Math.Log(positiveSigmoid)

                ' Compute the gradients and update the word embeddings.
                Dim targetGradient = contextEmbedding.Clone()
                Dim contextGradient = targetEmbedding.Clone()

                targetGradient = targetGradient.Select(Function(g) g * (positiveSigmoid - 1)).ToArray()
                contextGradient = contextGradient.Select(Function(g) g * (positiveSigmoid - 1)).ToArray()

                ' Update the word embeddings using the computed gradients.
                For i As Integer = 0 To EmbeddingSize - 1
                    targetEmbedding(i) -= LearningRate * targetGradient(i)
                    contextEmbedding(i) -= LearningRate * contextGradient(i)
                Next
            End Sub
        End Class
        Public Class WordEmbeddingsWithCBOW
            Inherits WordEmbeddingsModel

            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub

            Public Sub New(ByRef Vocabulary As List(Of String))
                MyBase.New(Vocabulary)
            End Sub

            Public Overrides Sub Train()
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Simulate training data (context pairs).
                Dim trainingData As New List(Of (List(Of String), String))()
                For i As Integer = 0 To Vocabulary.Count - 1
                    Dim contextWords As New List(Of String)()
                    For j As Integer = Math.Max(0, i - WindowSize) To Math.Min(Vocabulary.Count - 1, i + WindowSize)
                        If i <> j Then
                            contextWords.Add(Vocabulary(j))
                        End If
                    Next
                    If contextWords.Count > 0 Then
                        trainingData.Add((contextWords, Vocabulary(i)))
                    End If
                Next

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    trainingData = trainingData.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each context pair.
                    For Each item In trainingData
                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub

            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                ' Initialize word embeddings randomly.
                For Each word In Vocabulary
                    WordEmbeddings.Add(word, Enumerable.Range(0, EmbeddingSize).Select(Function(_i) Rand.NextDouble() - 0.5).ToArray())
                Next

                ' Simulate training data (context pairs).
                Dim trainingData As New List(Of (List(Of String), String))()
                For Each document In corpus
                    For wordIndex As Integer = 0 To document.Count - 1
                        Dim targetWord As String = document(wordIndex)
                        Dim contextStart As Integer = Math.Max(0, wordIndex - WindowSize)
                        Dim contextEnd As Integer = Math.Min(document.Count - 1, wordIndex + WindowSize)

                        Dim contextWords As New List(Of String)()
                        For contextIndex As Integer = contextStart To contextEnd
                            If contextIndex <> wordIndex Then
                                contextWords.Add(document(contextIndex))
                            End If
                        Next

                        trainingData.Add((contextWords, targetWord))
                    Next
                Next

                ' Training loop.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    trainingData = trainingData.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each context pair.
                    For Each item In trainingData
                        ' Compute the gradients and update the word embeddings.
                        Update(item.Item1, item.Item2)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary.
            End Sub

            Private Sub Update(contextWords As List(Of String), targetWord As String)
                Dim contextEmbeddings = contextWords.Select(Function(word) WordEmbeddings.GetVector(word)).ToList()
                Dim targetEmbedding = WordEmbeddings.GetVector(targetWord)

                ' Average the context embeddings.
                Dim averageContext = New Double(EmbeddingSize - 1) {}
                For Each context In contextEmbeddings
                    For i As Integer = 0 To EmbeddingSize - 1
                        averageContext(i) += context(i)
                    Next
                Next

                For i As Integer = 0 To EmbeddingSize - 1
                    averageContext(i) /= contextEmbeddings.Count
                Next

                ' Compute the loss for the target word.
                Dim targetLoss As Double = 0
                Dim positiveScore As Double = ComputeDotProduct(targetEmbedding, averageContext)
                Dim positiveSigmoid As Double = Sigmoid(positiveScore)
                targetLoss += -Math.Log(positiveSigmoid)

                ' Compute the gradient and update the word embeddings.
                Dim targetGradient = averageContext.Select(Function(g) g * (positiveSigmoid - 1)).ToArray()

                For Each context In contextEmbeddings
                    Dim sigmoidGradient As Double = (positiveSigmoid - 1.0) * LearningRate

                    For i As Integer = 0 To EmbeddingSize - 1
                        context(i) -= sigmoidGradient * targetEmbedding(i)
                        targetGradient(i) += sigmoidGradient * context(i)
                    Next
                Next

                ' Update the word embeddings using the computed gradients.
                For i As Integer = 0 To EmbeddingSize - 1
                    targetEmbedding(i) -= LearningRate * targetGradient(i)
                Next
            End Sub
        End Class
        Public Class WordEmbeddingWithTemplate
            Inherits WordEmbeddingsModel

            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub

            Public Sub New(ByRef Vocabulary As List(Of String))
                MyBase.New(Vocabulary)
            End Sub

            Public Overrides Sub Train()
                Throw New NotImplementedException()
            End Sub

            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                Throw New NotImplementedException()
            End Sub
        End Class
        Public Class WordEmbeddingWithSentiment
            Inherits WordEmbeddingsModel

            Private SentimentDictionary As New Dictionary(Of String, SentimentLabel)
            Private Shared PositiveSentiments As New List(Of String)
            Private Shared NegativeSentiments As New List(Of String)
            Private SentimentEmbeddings As New Dictionary(Of String, Double())

            Private Enum SentimentLabel
                Neutral
                Positive
                Negative
            End Enum

            ' WordSentiment class to store sentiment information for words.
            Private Class WordSentiment
                Public Property Word As String
                Public Property Sentiment As SentimentLabel
            End Class

            Private Sub InitializeVocab()
                ' Initialize sentiment dictionary with neutral sentiment for all words in the vocabulary.
                SentimentDictionary = New Dictionary(Of String, SentimentLabel)
                For Each item In WordEmbeddings.embeddings
                    SentimentDictionary.Add(item.Key, GetSentiment(item.Key))
                Next
            End Sub

            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub

            Public Function GetLabel(ByRef Word As String) As String
                Select Case GetSentiment(Word)
                    Case SentimentLabel.Negative
                        Return "Negative"
                    Case SentimentLabel.Positive
                        Return "Positive"
                    Case SentimentLabel.Neutral
                        Return "Neutral"
                End Select
                Return "Unknown"
            End Function

            Public Sub New(ByRef Vocabulary As List(Of String))
                MyBase.New(Vocabulary)
            End Sub

            Public Sub Initialize()
                LoadLists()
                InitializeVocab()
            End Sub

            ''' <summary>
            ''' Encode Sentiment into the Embedding model (does not affect the positions or order of the model)
            ''' </summary>
            Public Overrides Sub Train()
                Initialize()
                CreateSentimentEmbeddings()
                UpdateWordEmbeddings()
            End Sub
            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                Initialize()
                CreateSentimentEmbeddings()
                UpdateWordEmbeddings()

                ' Now you can continue training the word embeddings using the corpus data.
                ' You can add the training loop here based on your specific model requirements.
                ' The previous part of the code initializes the sentiment embeddings, which can be used
                ' in combination with the word embeddings during the training process.
                ' You can perform additional iterations or training steps to fine-tune the embeddings
                ' based on your specific training algorithm.

                ' Training loop on the corpus data.
                For epoch As Integer = 1 To NumEpochs
                    Console.WriteLine($"Training Epoch {epoch}/{NumEpochs}")

                    ' Shuffle the training data to avoid learning order biases.
                    ' Modify the training data to suit your specific corpus structure and context pairs.
                    Dim trainingData As New List(Of (List(Of String), String))()
                    For Each document In corpus
                        For wordIndex As Integer = 0 To document.Count - 1
                            Dim contextWords As New List(Of String)()
                            Dim targetWord As String = document(wordIndex)
                            Dim contextStart As Integer = Math.Max(0, wordIndex - WindowSize)
                            Dim contextEnd As Integer = Math.Min(document.Count - 1, wordIndex + WindowSize)

                            For contextIndex As Integer = contextStart To contextEnd
                                If contextIndex = wordIndex Then Continue For ' Skip the target word itself.
                                contextWords.Add(document(contextIndex))
                            Next

                            If contextWords.Count > 0 Then
                                trainingData.Add((contextWords, targetWord))
                            End If
                        Next
                    Next

                    ' Shuffle the training data.
                    trainingData = trainingData.OrderBy(Function(_item) Rand.Next()).ToList()

                    ' Gradient descent for each context pair.
                    For Each item In trainingData
                        ' Compute the gradients and update the word embeddings.
                        UpdateWordEmbeddings(item.Item1, item.Item2)
                    Next
                Next

                ' Print the learned word embeddings.
                For Each word In Vocabulary
                    Console.WriteLine($"{word}: {String.Join(", ", WordEmbeddings.GetVector(word))}")
                Next

                ' Now you have learned word embeddings for the given vocabulary and trained them on the corpus data.
            End Sub

            Private Sub UpdateWordEmbeddings(contextWords As List(Of String), targetWord As String)
                Dim contextEmbeddings = contextWords.Select(Function(word) WordEmbeddings.GetVector(word)).ToList()
                Dim targetEmbedding = WordEmbeddings.GetVector(targetWord)

                ' Average the context embeddings.
                Dim averageContext = New Double(EmbeddingSize - 1) {}
                For Each context In contextEmbeddings
                    For i As Integer = 0 To EmbeddingSize - 1
                        averageContext(i) += context(i)
                    Next
                Next

                For i As Integer = 0 To EmbeddingSize - 1
                    averageContext(i) /= contextEmbeddings.Count
                Next

                ' Compute the loss for the target word.
                Dim targetLoss As Double = 0
                Dim positiveScore As Double = ComputeDotProduct(targetEmbedding, averageContext)
                Dim positiveSigmoid As Double = Sigmoid(positiveScore)
                targetLoss += -Math.Log(positiveSigmoid)

                ' Compute the gradient and update the word embeddings.
                Dim targetGradient = averageContext.Select(Function(g) g * (positiveSigmoid - 1)).ToArray()

                For Each context In contextEmbeddings
                    Dim sigmoidGradient As Double = (positiveSigmoid - 1.0) * LearningRate

                    For i As Integer = 0 To EmbeddingSize - 1
                        context(i) -= sigmoidGradient * targetEmbedding(i)
                        targetGradient(i) += sigmoidGradient * context(i)
                    Next
                Next

                ' Update the word embeddings using the computed gradients.
                For i As Integer = 0 To EmbeddingSize - 1
                    targetEmbedding(i) -= LearningRate * targetGradient(i)
                Next
            End Sub

            Public Shared Function CombineVectors(vector1 As Double(), vector2 As Double()) As Double()
                ' Combine two vectors element-wise
                Dim combinedVector As Double() = New Double(vector1.Length - 1) {}
                For i As Integer = 0 To vector1.Length - 1
                    combinedVector(i) = vector1(i) + vector2(i)
                Next
                Return combinedVector
            End Function

            Private Sub UpdateWordEmbeddings()
                Dim CurrentEmbeddings = WordEmbeddings.embeddings
                Dim NewEmbeddings As New Dictionary(Of String, Double())

                For Each item In SentimentEmbeddings
                    Dim NewVector = CombineVectors(item.Value, WordEmbeddings.GetVector(item.Key))
                    NewEmbeddings.Add(item.Key, NewVector)
                Next
                WordEmbeddings.embeddings = NewEmbeddings
            End Sub

            Private Shared Function GetSentiment(ByRef Word As String) As SentimentLabel
                For Each item In PositiveSentiments
                    If item = Word Then
                        Return SentimentLabel.Positive
                    End If
                Next

                For Each item In NegativeSentiments
                    If item = Word Then
                        Return SentimentLabel.Negative
                    End If
                Next

                Return SentimentLabel.Neutral
            End Function

            Private Function OneShotEncode(ByVal label As SentimentLabel) As Double()
                ' One-shot encode the sentiment label into a binary vector
                ' In this example, we'll represent the label with a 3-bit binary code
                Dim encodedVector As Double() = New Double(2) {} ' 3-bit binary code (0, 0, 0)

                Select Case label
                    Case SentimentLabel.Positive
                        encodedVector(0) = 1 ' 3-bit binary code (1, 0, 0)
                    Case SentimentLabel.Negative
                        encodedVector(1) = 1 ' 3-bit binary code (0, 1, 0)
                    Case SentimentLabel.Neutral
                        encodedVector(2) = 1 ' 3-bit binary code (0, 0, 1)
                End Select

                Return encodedVector
            End Function

            Private Sub CreateSentimentEmbeddings()
                For Each item In SentimentDictionary
                    SentimentEmbeddings.Add(item.Key, OneShotEncode(item.Value))
                Next
            End Sub

            Private Sub LoadLists()
                PositiveSentiments = LoadList("PositiveSent.txt")
                NegativeSentiments = LoadList("NegativeSent.txt")
            End Sub

            Private Function LoadList(ByRef FileName As String) As List(Of String)
                Dim corpusRoot As String = Application.StartupPath & "\data\"
                Dim wordlistPath As String = Path.Combine(corpusRoot, FileName)
                Dim wordlistReader As New Functions.WordListReader(wordlistPath)
                Dim Lst = wordlistReader.GetWords()
                Return Lst
            End Function
        End Class

        Public Class WordEmbeddingWithTfIdf
            Inherits WordEmbeddingsModel

            Public Sub New(ByRef model As WordEmbeddingsModel)
                MyBase.New(model)
            End Sub

            Public Sub New(ByRef Vocabulary As List(Of String))
                MyBase.New(Vocabulary)
            End Sub
            Public Overrides Sub Train(corpus As List(Of List(Of String)))
                ' Assuming you have pre-trained word embeddings stored in the 'WordEmbeddings.embeddings' variable.

                ' Step 1: Calculate term frequency (TF) for each term in the vocabulary.
                Dim termFrequency As New Dictionary(Of String, Integer)()
                For Each sentence In corpus
                    For Each term In sentence
                        If termFrequency.ContainsKey(term) Then
                            termFrequency(term) += 1
                        Else
                            termFrequency(term) = 1
                        End If
                    Next
                Next

                ' Step 2: Sort the vocabulary based on term frequency in descending order (highest ranked terms first).
                Vocabulary = termFrequency.OrderByDescending(Function(entry) entry.Value).Select(Function(entry) entry.Key).ToList()

                ' Step 3: Create a SentenceVectorizer using the sorted vocabulary to calculate TF-IDF scores.
                Dim sentenceVectorizer As New Functions.SentenceVectorizer(corpus)

                ' Step 4: Calculate TF-IDF vectors for each term in the sorted vocabulary.
                Dim tfidfWeightedEmbeddings As New Dictionary(Of String, List(Of Double))
                For Each term In Vocabulary
                    Dim tfidfVector As List(Of Double) = sentenceVectorizer.Vectorize(New List(Of String) From {term})
                    Dim wordEmbedding() As Double = WordEmbeddings.embeddings(term)

                    ' Multiply the word embedding by the corresponding TF-IDF score to get the weighted word embedding.
                    Dim weightedEmbedding As List(Of Double) = wordEmbedding.Select(Function(val, idx) val * tfidfVector(idx)).ToList()

                    ' Store the weighted embedding in the dictionary.
                    tfidfWeightedEmbeddings(term) = weightedEmbedding
                Next

                ' Step 5: Store the TF-IDF weighted word embeddings in the WordEmbedding class.
                For Each term In tfidfWeightedEmbeddings.Keys
                    WordEmbeddings.Add(term, tfidfWeightedEmbeddings(term).ToArray())
                Next
            End Sub


            Public Overrides Sub Train()
                ' Assuming you have pre-trained word embeddings stored in the 'WordEmbeddings.embeddings' variable.

                ' Step 1: Calculate term frequency (TF) for each term in the vocabulary.
                Dim termFrequency As New Dictionary(Of String, Integer)()
                For Each term In Vocabulary
                    termFrequency(term) = 0
                Next

                ' Count the occurrences of each term in the vocabulary.
                For Each sentence In Vocabulary
                    For Each term In sentence
                        If termFrequency.ContainsKey(term) Then
                            termFrequency(term) += 1
                        End If
                    Next
                Next

                ' Step 2: Sort the vocabulary based on term frequency in descending order (highest ranked terms first).
                Vocabulary = Vocabulary.OrderByDescending(Function(term) termFrequency(term)).ToList()

                ' Step 3: Create a SentenceVectorizer using the sorted vocabulary to calculate TF-IDF scores.
                Dim sentenceVectorizer As New Functions.SentenceVectorizer(New List(Of List(Of String)) From {Vocabulary})

                ' Step 4: Calculate TF-IDF vectors for each term in the sorted vocabulary.
                Dim tfidfWeightedEmbeddings As New Dictionary(Of String, List(Of Double))
                For Each term In Vocabulary
                    Dim tfidfVector As List(Of Double) = sentenceVectorizer.Vectorize(New List(Of String) From {term})
                    Dim wordEmbedding() As Double = WordEmbeddings.embeddings(term)

                    ' Multiply the word embedding by the corresponding TF-IDF score to get the weighted word embedding.
                    Dim weightedEmbedding As List(Of Double) = wordEmbedding.Select(Function(val, idx) val * tfidfVector(idx)).ToList()

                    ' Store the weighted embedding in the dictionary.
                    tfidfWeightedEmbeddings(term) = weightedEmbedding
                Next

                ' Step 5: Store the TF-IDF weighted word embeddings in the WordEmbedding class.
                For Each term In tfidfWeightedEmbeddings.Keys
                    WordEmbeddings.Add(term, tfidfWeightedEmbeddings(term).ToArray())
                Next
            End Sub


        End Class

    End Namespace
    Namespace Functions
        ''' <summary>
        ''' This is a TFIDF Vectorizer For basic Embeddings
        ''' </summary>
        Public Class SentenceVectorizer
            Private ReadOnly documents As List(Of List(Of String))
            Private ReadOnly idf As Dictionary(Of String, Double)

            Public Sub New(documents As List(Of List(Of String)))
                Me.documents = documents
                Me.idf = CalculateIDF(documents)
            End Sub

            Public Sub New()
                documents = New List(Of List(Of String))
                idf = New Dictionary(Of String, Double)
            End Sub

            Public Function Vectorize(sentence As List(Of String)) As List(Of Double)
                Dim termFrequency = CalculateTermFrequency(sentence)
                Dim vector As New List(Of Double)

                For Each term In idf.Keys
                    Dim tfidf As Double = termFrequency(term) * idf(term)
                    vector.Add(tfidf)
                Next

                Return vector
            End Function

            Public Function CalculateIDF(documents As List(Of List(Of String))) As Dictionary(Of String, Double)
                Dim idf As New Dictionary(Of String, Double)
                Dim totalDocuments As Integer = documents.Count

                For Each document In documents
                    Dim uniqueTerms As List(Of String) = document.Distinct().ToList()

                    For Each term In uniqueTerms
                        If idf.ContainsKey(term) Then
                            idf(term) += 1
                        Else
                            idf(term) = 1
                        End If
                    Next
                Next

                For Each term In idf.Keys
                    idf(term) = Math.Log(totalDocuments / idf(term))
                Next

                Return idf
            End Function

            Public Function CalculateTermFrequency(sentence As List(Of String)) As Dictionary(Of String, Double)
                Dim termFrequency As New Dictionary(Of String, Double)

                For Each term In sentence
                    If termFrequency.ContainsKey(term) Then
                        termFrequency(term) += 1
                    Else
                        termFrequency(term) = 1
                    End If
                Next

                Return termFrequency
            End Function

        End Class
        Public Class VocabularyBuilder
            Private embeddingMatrix As Double(,)
            Private embeddingSize As Integer
            Private iterations As Integer
            Private Function GetEmbedding(index As Integer) As Double()
                If indexToWord.ContainsKey(index) Then
                    Dim vector(embeddingSize - 1) As Double
                    For i As Integer = 0 To embeddingSize - 1
                        vector(i) = embeddingMatrix(index, i)
                    Next
                    Return vector
                Else
                    Return Nothing
                End If
            End Function
            Public Function GenerateCooccurrenceMatrix(corpus As String(), windowSize As Integer) As Dictionary(Of String, Dictionary(Of String, Double))
                Dim matrix As New Dictionary(Of String, Dictionary(Of String, Double))

                For Each sentence In corpus
                    Dim words As String() = sentence.Split(" "c)
                    Dim length As Integer = words.Length

                    For i As Integer = 0 To length - 1
                        Dim targetWord As String = words(i)

                        If Not matrix.ContainsKey(targetWord) Then
                            matrix(targetWord) = New Dictionary(Of String, Double)
                        End If

                        For j As Integer = Math.Max(0, i - windowSize) To Math.Min(length - 1, i + windowSize)
                            If i = j Then
                                Continue For
                            End If

                            Dim contextWord As String = words(j)
                            Dim distance As Double = 1 / Math.Abs(i - j)

                            If matrix(targetWord).ContainsKey(contextWord) Then
                                matrix(targetWord)(contextWord) += distance
                            Else
                                matrix(targetWord)(contextWord) = distance
                            End If
                        Next
                    Next
                Next

                Return matrix
            End Function
            Public Model As New Dictionary(Of String, Dictionary(Of String, Double))
            Private windowSize As Integer
            Public Sub Train(corpus As String(), ByRef WindowSize As Integer, ByRef Iterations As Integer)
                BuildVocabulary(corpus.ToList)
                InitializeEmbeddings()
                Model = GenerateCooccurrenceMatrix(corpus, WindowSize)

                For iteration As Integer = 1 To Iterations
                    For Each targetWord In Model.Keys
                        Dim targetIndex As Integer = GetOrCreateWordIndex(targetWord)
                        Dim targetEmbedding As Double() = GetEmbedding(targetIndex)

                        For Each contextWord In Model(targetWord).Keys
                            Dim contextIndex As Integer = GetOrCreateWordIndex(contextWord)
                            Dim contextEmbedding As Double() = GetEmbedding(contextIndex)
                            Dim cooccurrenceValue As Double = Model(targetWord)(contextWord)
                            Dim weight As Double = Math.Log(cooccurrenceValue)

                            For i As Integer = 0 To embeddingSize - 1
                                targetEmbedding(i) += weight * contextEmbedding(i)
                            Next
                        Next
                    Next
                Next
                Model = PMI.CalculatePMI(Model)
            End Sub
            Public Sub Train(corpus As String(), iterations As Integer, ByRef LearningRate As Double)

                Me.iterations = iterations
                Model = GenerateCooccurrenceMatrix(corpus, windowSize)
                InitializeEmbeddings()

                For iteration As Integer = 1 To iterations
                    For Each targetWord In Model.Keys
                        If wordToIndex.ContainsKey(targetWord) Then
                            Dim targetIndex As Integer = wordToIndex(targetWord)
                            Dim targetEmbedding As Double() = GetEmbedding(targetIndex)

                            ' Initialize gradient accumulator for target embedding
                            Dim gradTarget As Double() = New Double(embeddingSize - 1) {}

                            For Each contextWord In Model(targetWord).Keys
                                If wordToIndex.ContainsKey(contextWord) Then
                                    Dim contextIndex As Integer = wordToIndex(contextWord)
                                    Dim contextEmbedding As Double() = GetEmbedding(contextIndex)
                                    Dim cooccurrenceValue As Double = Model(targetWord)(contextWord)
                                    Dim weight As Double = Math.Log(cooccurrenceValue)

                                    ' Initialize gradient accumulator for context embedding
                                    Dim gradContext As Double() = New Double(embeddingSize - 1) {}

                                    ' Calculate the gradients
                                    For i As Integer = 0 To embeddingSize - 1
                                        Dim gradCoefficient As Double = weight * targetEmbedding(i)

                                        gradTarget(i) = LearningRate * gradCoefficient
                                        gradContext(i) = LearningRate * gradCoefficient
                                    Next

                                    ' Update the target and context embeddings
                                    For i As Integer = 0 To embeddingSize - 1
                                        targetEmbedding(i) += gradTarget(i)
                                        contextEmbedding(i) += gradContext(i)
                                    Next
                                End If
                            Next
                        End If
                    Next
                Next
            End Sub


            Private Sub InitializeEmbeddings()
                Dim vocabSize As Integer = vocabulary.Count
                embeddingMatrix = New Double(vocabSize - 1, embeddingSize - 1) {}

                Dim random As New Random()
                For i As Integer = 0 To vocabSize - 1
                    For j As Integer = 0 To embeddingSize - 1
                        embeddingMatrix(i, j) = random.NextDouble()
                    Next
                Next
            End Sub
            Private Function CalculateSimilarity(vectorA As Double(), vectorB As Double()) As Double
                Dim dotProduct As Double = 0
                Dim magnitudeA As Double = 0
                Dim magnitudeB As Double = 0

                For i As Integer = 0 To vectorA.Length - 1
                    dotProduct += vectorA(i) * vectorB(i)
                    magnitudeA += vectorA(i) * vectorA(i)
                    magnitudeB += vectorB(i) * vectorB(i)
                Next

                If magnitudeA <> 0 AndAlso magnitudeB <> 0 Then
                    Return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB))
                Else
                    Return 0
                End If
            End Function
            ''' <summary>
            ''' Discovers collocations among the specified words based on the trained model.
            ''' </summary>
            ''' <param name="words">The words to discover collocations for.</param>
            ''' <param name="threshold">The similarity threshold for collocation discovery.</param>
            ''' <returns>A list of collocations (word pairs) that meet the threshold.</returns>
            Public Function DiscoverCollocations(words As String(), threshold As Double) As List(Of Tuple(Of String, String))
                Dim collocations As New List(Of Tuple(Of String, String))

                For i As Integer = 0 To words.Length - 2
                    For j As Integer = i + 1 To words.Length - 1
                        Dim word1 As String = words(i)
                        Dim word2 As String = words(j)

                        If vocabulary.Contains(word1) AndAlso vocabulary.Contains(word2) Then
                            Dim vector1 As Double() = GetEmbedding(wordToIndex(word1))
                            Dim vector2 As Double() = GetEmbedding(wordToIndex(word2))
                            Dim similarity As Double = CalculateSimilarity(vector1, vector2)

                            If similarity >= threshold Then
                                collocations.Add(Tuple.Create(word1, word2))
                            End If
                        End If
                    Next
                Next

                Return collocations
            End Function
            ''' <summary>
            ''' Gets the most similar words to the specified word.
            ''' </summary>
            ''' <param name="word">The target word.</param>
            ''' <param name="topK">The number of similar words to retrieve.</param>
            ''' <returns>A list of the most similar words.</returns>
            Public Function GetMostSimilarWords(word As String, topK As Integer) As List(Of String)
                Dim wordIndex As Integer = wordToIndex(word)

                Dim similarities As New Dictionary(Of String, Double)()
                For Each otherWord As String In vocabulary
                    If otherWord <> word Then
                        Dim otherWordIndex As Integer = wordToIndex(otherWord)
                        Dim similarity As Double = CalculateSimilarity(GetEmbedding(wordIndex), GetEmbedding(otherWordIndex))
                        similarities.Add(otherWord, similarity)
                    End If
                Next

                Dim orderedSimilarities = similarities.OrderByDescending(Function(x) x.Value)
                Dim mostSimilarWords As New List(Of String)()

                Dim count As Integer = 0
                For Each pair In orderedSimilarities
                    mostSimilarWords.Add(pair.Key)
                    count += 1
                    If count >= topK Then
                        Exit For
                    End If
                Next

                Return mostSimilarWords
            End Function
            Private vocabulary As HashSet(Of String)
            Private wordToIndex As Dictionary(Of String, Integer)
            Private indexToWord As Dictionary(Of Integer, String)
            Public Function BuildVocabulary(corpus As List(Of String)) As HashSet(Of String)

                Dim index As Integer = 0
                For Each sentence As String In corpus
                    Dim cleanedText As String = Regex.Replace(sentence, "[^\w\s]", "").ToLower()
                    Dim tokens As String() = cleanedText.Split()
                    For Each token As String In tokens
                        If Not vocabulary.Contains(token) Then
                            vocabulary.Add(token)
                            wordToIndex.Add(token, index)
                            indexToWord.Add(index, token)
                            index += 1
                        End If
                    Next
                Next
                Return vocabulary
            End Function
            Public Function GetOrCreateWordIndex(word As String) As Integer
                If wordToIndex.ContainsKey(word) Then
                    Return wordToIndex(word)
                Else
                    Dim newIndex As Integer = vocabulary.Count
                    vocabulary.Add(word)
                    wordToIndex.Add(word, newIndex)
                    indexToWord.Add(newIndex, word)
                    Return newIndex
                End If
            End Function
            Public Function DisplayMatrix(matrix As Dictionary(Of String, Dictionary(Of String, Double))) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Object)
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Object = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function
            Public Sub DisplayModel()
                DisplayMatrix(Model)
            End Sub
            Public Function DisplayMatrix(matrix As Dictionary(Of String, Dictionary(Of String, Integer))) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Object)()
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Integer = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function

        End Class
        Public Class PMI
            ''' <summary>
            ''' Calculates the Pointwise Mutual Information (PMI) matrix for the trained model.
            ''' </summary>
            ''' <returns>A dictionary representing the PMI matrix.</returns>
            Public Shared Function CalculatePMI(ByRef model As Dictionary(Of String, Dictionary(Of String, Double))) As Dictionary(Of String, Dictionary(Of String, Double))
                Dim pmiMatrix As New Dictionary(Of String, Dictionary(Of String, Double))

                Dim totalCooccurrences As Double = GetTotalCooccurrences(model)

                For Each targetWord In model.Keys
                    Dim targetOccurrences As Double = GetTotalOccurrences(targetWord, model)

                    If Not pmiMatrix.ContainsKey(targetWord) Then
                        pmiMatrix(targetWord) = New Dictionary(Of String, Double)
                    End If

                    For Each contextWord In model(targetWord).Keys
                        Dim contextOccurrences As Double = GetTotalOccurrences(contextWord, model)
                        Dim cooccurrences As Double = model(targetWord)(contextWord)

                        Dim pmiValue As Double = Math.Log((cooccurrences * totalCooccurrences) / (targetOccurrences * contextOccurrences))
                        pmiMatrix(targetWord)(contextWord) = pmiValue
                    Next
                Next

                Return pmiMatrix
            End Function
            Public Shared Function GetTotalCooccurrences(ByRef Model As Dictionary(Of String, Dictionary(Of String, Double))) As Double
                Dim total As Double = 0

                For Each targetWord In Model.Keys
                    For Each cooccurrenceValue In Model(targetWord).Values
                        total += cooccurrenceValue
                    Next
                Next

                Return total
            End Function
            Public Shared Function GetTotalOccurrences(word As String, ByRef Model As Dictionary(Of String, Dictionary(Of String, Double))) As Double
                Dim total As Double = 0

                If Model.ContainsKey(word) Then
                    total = Model(word).Values.Sum()
                End If

                Return total
            End Function
            Public Shared Function CalculateCosineSimilarity(vectorA As Double(), vectorB As Double()) As Double
                Dim dotProduct As Double = 0
                Dim magnitudeA As Double = 0
                Dim magnitudeB As Double = 0

                For i As Integer = 0 To vectorA.Length - 1
                    dotProduct += vectorA(i) * vectorB(i)
                    magnitudeA += vectorA(i) * vectorA(i)
                    magnitudeB += vectorB(i) * vectorB(i)
                Next

                If magnitudeA <> 0 AndAlso magnitudeB <> 0 Then
                    Return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB))
                Else
                    Return 0
                End If
            End Function
            Public Shared Function GenerateCooccurrenceMatrix(corpus As String(), windowSize As Integer) As Dictionary(Of String, Dictionary(Of String, Double))
                Dim matrix As New Dictionary(Of String, Dictionary(Of String, Double))

                For Each sentence In corpus
                    Dim words As String() = sentence.Split(" "c)
                    Dim length As Integer = words.Length

                    For i As Integer = 0 To length - 1
                        Dim targetWord As String = words(i)

                        If Not matrix.ContainsKey(targetWord) Then
                            matrix(targetWord) = New Dictionary(Of String, Double)
                        End If

                        For j As Integer = Math.Max(0, i - windowSize) To Math.Min(length - 1, i + windowSize)
                            If i = j Then
                                Continue For
                            End If

                            Dim contextWord As String = words(j)
                            Dim distance As Double = 1 / Math.Abs(i - j)

                            If matrix(targetWord).ContainsKey(contextWord) Then
                                matrix(targetWord)(contextWord) += distance
                            Else
                                matrix(targetWord)(contextWord) = distance
                            End If
                        Next
                    Next
                Next

                Return matrix
            End Function

        End Class
        Public Class WordListReader
            Private wordList As List(Of String)

            Public Sub New(filePath As String)
                wordList = New List(Of String)()
                ReadWordList(filePath)
            End Sub

            Private Sub ReadWordList(filePath As String)
                Using reader As New StreamReader(filePath)
                    While Not reader.EndOfStream
                        Dim line As String = reader.ReadLine()
                        If Not String.IsNullOrEmpty(line) Then
                            wordList.Add(line.Trim.ToLower)
                        End If
                    End While
                End Using
            End Sub

            Public Function GetWords() As List(Of String)
                Return wordList
            End Function
            ' Usage Example:
            Public Shared Sub Main()
                ' Assuming you have a wordlist file named 'words.txt' in the same directory
                Dim corpusRoot As String = "."
                Dim wordlistPath As String = Path.Combine(corpusRoot, "wordlist.txt")

                Dim wordlistReader As New WordListReader(wordlistPath)
                Dim words As List(Of String) = wordlistReader.GetWords()

                For Each word As String In words
                    Console.WriteLine(word)
                Next
                Console.ReadLine()
                ' Rest of your code...
            End Sub


        End Class
        ''' <summary>
        ''' Returns a list WordGram Probability Given a Sequence of Tokens 
        ''' </summary>
        Public Class Wordgram
            Private n As Integer
            Public Shared Sub Main()
                ' Train the wordgram model
                Dim trainingData As New List(Of String) From {"I love cats and dogs.", "Dogs are loyal companions."}
                Dim words As New List(Of String) From {
            "apple", "banana", "orange", "apple", "pear", "kiwi", "orange", "mango", "kiwi", "guava", "kiwi", "orange", "orange", "apple", "banana"
        }
                Dim sentences As New List(Of String) From {
            "I love apples.",
            "Bananas are tasty.",
            "I love apples.",
            "I enjoy eating bananas.",
            "mango is a delicious fruit.", "Bananas are tasty.",
            "I love apples.", "I enjoy eating bananas.",
            "Kiwi is a delicious fruit.", "I love apples.",
            "I enjoy eating bananas.",
            "orange is a delicious fruit.", "I love apples.",
            "I enjoy eating bananas.",
            "Kiwi is a delicious fruit.", "Bananas are tasty."
        }
                Dim Corpus As New List(Of String)
                Corpus.AddRange(sentences)
                Corpus.AddRange(words)


                ' Generate a sentence using the wordgram model
                For I = 1 To 5
                    Dim wordgramModel As New Wordgram(Corpus, I)
                    Dim generatedSentence As String = wordgramModel.GenerateSentence()
                    Console.WriteLine(generatedSentence)
                Next I
                Console.ReadLine()
            End Sub

            Public wordgramCounts As New Dictionary(Of List(Of String), Integer)
            Public wordgramProbabilities As New Dictionary(Of List(Of String), Double)
            Public Sub New(trainingData As List(Of String), n As Integer)
                Me.n = n
                TrainModel(trainingData)
            End Sub
            Private Sub TrainModel(trainingData As List(Of String))
                ' Preprocess training data and tokenize into wordgrams
                Dim wordgrams As New List(Of List(Of String))
                For Each sentence As String In trainingData
                    Dim tokens() As String = sentence.Split(" "c)
                    For i As Integer = 0 To tokens.Length - n
                        Dim wordgram As List(Of String) = tokens.Skip(i).Take(n).ToList()
                        wordgrams.Add(wordgram)
                    Next
                Next

                ' Count wordgrams
                For Each wordgram As List(Of String) In wordgrams
                    If wordgramCounts.ContainsKey(wordgram) Then
                        wordgramCounts(wordgram) += 1
                    Else
                        wordgramCounts.Add(wordgram, 1)
                    End If
                Next

                ' Calculate wordgram probabilities
                Dim totalCount As Integer = wordgramCounts.Values.Sum()
                For Each wordgram As List(Of String) In wordgramCounts.Keys
                    Dim count As Integer = wordgramCounts(wordgram)
                    Dim probability As Double = count / totalCount
                    wordgramProbabilities.Add(wordgram, probability)
                Next
            End Sub
            Private Function GenerateNextWord(wordgram As List(Of String)) As String
                Dim random As New Random()
                Dim candidates As New List(Of String)
                Dim probabilities As New List(Of Double)

                ' Collect candidate words and their probabilities
                For Each candidateWordgram As List(Of String) In wordgramCounts.Keys
                    If candidateWordgram.GetRange(0, n - 1).SequenceEqual(wordgram) Then
                        Dim candidateWord As String = candidateWordgram.Last()
                        Dim probability As Double = wordgramProbabilities(candidateWordgram)
                        candidates.Add(candidateWord)
                        probabilities.Add(probability)
                    End If
                Next

                ' Randomly select a candidate word based on probabilities
                Dim totalProbability As Double = probabilities.Sum()
                Dim randomValue As Double = random.NextDouble() * totalProbability
                Dim cumulativeProbability As Double = 0

                For i As Integer = 0 To candidates.Count - 1
                    cumulativeProbability += probabilities(i)
                    If randomValue <= cumulativeProbability Then
                        Return candidates(i)
                    End If
                Next

                Return ""
            End Function
            Public Function GenerateSentence() As String
                Dim sentence As New List(Of String)
                Dim random As New Random()

                ' Start the sentence with a random wordgram
                Dim randomIndex As Integer = random.Next(0, wordgramCounts.Count)
                Dim currentWordgram As List(Of String) = wordgramCounts.Keys(randomIndex)
                sentence.AddRange(currentWordgram)

                ' Generate subsequent words based on wordgram probabilities
                While wordgramCounts.ContainsKey(currentWordgram)
                    Dim nextWord As String = GenerateNextWord(currentWordgram)
                    If nextWord = "" Then
                        Exit While
                    End If
                    sentence.Add(nextWord)

                    ' Backoff to lower-order wordgrams if necessary
                    If currentWordgram.Count > 1 Then
                        currentWordgram.RemoveAt(0)
                    Else
                        Exit While
                    End If
                    currentWordgram.Add(nextWord)
                End While

                Return String.Join(" ", sentence)
            End Function
            Private Sub Train(trainingData As List(Of String))
                ' Preprocess training data and tokenize into wordgrams
                Dim wordgrams As New List(Of List(Of String))
                For Each sentence As String In trainingData
                    Dim tokens() As String = sentence.Split(" "c)
                    For i As Integer = 0 To tokens.Length - n
                        Dim wordgram As List(Of String) = tokens.Skip(i).Take(n).ToList()
                        wordgrams.Add(wordgram)
                    Next
                Next

                ' Count wordgrams
                For Each wordgram As List(Of String) In wordgrams
                    If wordgramCounts.ContainsKey(wordgram) Then
                        wordgramCounts(wordgram) += 1
                    Else
                        wordgramCounts.Add(wordgram, 1)
                    End If
                Next

                ' Calculate wordgram probabilities based on frequency-based distribution
                For Each wordgram As List(Of String) In wordgramCounts.Keys
                    Dim count As Integer = wordgramCounts(wordgram)
                    Dim order As Integer = wordgram.Count

                    ' Calculate the frequency threshold for higher-order n-grams
                    Dim frequencyThreshold As Integer = 5 ' Set your desired threshold
                    If order = n AndAlso count >= frequencyThreshold Then
                        wordgramProbabilities.Add(wordgram, count)
                    ElseIf order < n AndAlso count >= frequencyThreshold Then
                        ' Assign the frequency to lower-order n-grams
                        Dim lowerOrderWordgram As List(Of String) = wordgram.Skip(1).ToList()
                        If wordgramProbabilities.ContainsKey(lowerOrderWordgram) Then
                            wordgramProbabilities(lowerOrderWordgram) += count
                        Else
                            wordgramProbabilities.Add(lowerOrderWordgram, count)
                        End If
                    End If
                Next

                ' Normalize probabilities within each order
                For order As Integer = 1 To n
                    Dim totalProbability As Double = 0
                    For Each wordgram As List(Of String) In wordgramProbabilities.Keys.ToList()
                        If wordgram.Count = order Then
                            totalProbability += wordgramProbabilities(wordgram)
                        End If
                    Next
                    For Each wordgram As List(Of String) In wordgramProbabilities.Keys.ToList()
                        If wordgram.Count = order Then
                            wordgramProbabilities(wordgram) /= totalProbability
                        End If
                    Next
                Next
            End Sub


        End Class
        Public Class Co_Occurrence_Matrix
            Public Shared Function PrintOccurrenceMatrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer)), entityList As List(Of String)) As String
                ' Prepare the header row
                Dim headerRow As String = "|               |"

                For Each entity As String In entityList
                    If coOccurrenceMatrix.ContainsKey(entity) Then
                        headerRow &= $" [{entity}] ({coOccurrenceMatrix(entity).Count}) |"
                    End If
                Next

                Dim str As String = ""
                ' Print the header row
                Console.WriteLine(headerRow)

                str &= headerRow & vbNewLine
                ' Print the co-occurrence matrix
                For Each entity As String In coOccurrenceMatrix.Keys
                    Dim rowString As String = $"| [{entity}] ({coOccurrenceMatrix(entity).Count})        |"

                    For Each coOccurringEntity As String In entityList
                        Dim count As Integer = 0
                        If coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                            count = coOccurrenceMatrix(entity)(coOccurringEntity)
                        End If
                        rowString &= $"{count.ToString().PadLeft(7)} "
                    Next

                    Console.WriteLine(rowString)
                    str &= rowString & vbNewLine
                Next
                Return str
            End Function

            ''' <summary>
            ''' The co-occurrence matrix shows the frequency of co-occurrences between different entities in the given text. Each row represents an entity, and each column represents another entity. The values in the matrix indicate how many times each entity appeared within the specified window size of the other entities. A value of 0 means that the two entities did not co-occur within the given window size.
            ''' </summary>
            ''' <param name="text"></param>
            ''' <param name="entityList"></param>
            ''' <param name="windowSize"></param>
            ''' <returns></returns>
            Public Shared Function iCoOccurrenceMatrix(text As String, entityList As List(Of String), windowSize As Integer) As Dictionary(Of String, Dictionary(Of String, Integer))
                Dim coOccurrenceMatrix As New Dictionary(Of String, Dictionary(Of String, Integer))

                Dim words() As String = text.ToLower().Split(" "c) ' Convert the text to lowercase here
                For i As Integer = 0 To words.Length - 1
                    If entityList.Contains(words(i)) Then
                        Dim entity As String = words(i)
                        If Not coOccurrenceMatrix.ContainsKey(entity) Then
                            coOccurrenceMatrix(entity) = New Dictionary(Of String, Integer)()
                        End If

                        For j As Integer = i - windowSize To i + windowSize
                            If j >= 0 AndAlso j < words.Length AndAlso i <> j AndAlso entityList.Contains(words(j)) Then
                                Dim coOccurringEntity As String = words(j)
                                If Not coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                                    coOccurrenceMatrix(entity)(coOccurringEntity) = 0
                                End If

                                coOccurrenceMatrix(entity)(coOccurringEntity) += 1
                            End If
                        Next
                    End If
                Next

                Return coOccurrenceMatrix
            End Function

            ''' <summary>
            ''' The PMI matrix measures the statistical association or co-occurrence patterns between different entities in the text. It is calculated based on the co-occurrence matrix. PMI values are used to assess how much more likely two entities are to co-occur together than they would be if their occurrences were independent of each other.
            '''
            '''  positive PMI value indicates that the two entities are likely To co-occur more often than expected by chance, suggesting a positive association between them.
            '''  PMI value Of 0 means that the two entities co-occur As often As expected by chance, suggesting no significant association.
            '''  negative PMI value indicates that the two entities are less likely To co-occur than expected by chance, suggesting a negative association Or avoidance.
            ''' </summary>
            ''' <param name="coOccurrenceMatrix"></param>
            ''' <returns></returns>
            Public Shared Function CalculatePMI(coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer))) As Dictionary(Of String, Dictionary(Of String, Double))
                Dim pmiMatrix As New Dictionary(Of String, Dictionary(Of String, Double))

                For Each entity As String In coOccurrenceMatrix.Keys
                    Dim entityOccurrences As Integer = coOccurrenceMatrix(entity).Sum(Function(kv) kv.Value)

                    If Not pmiMatrix.ContainsKey(entity) Then
                        pmiMatrix(entity) = New Dictionary(Of String, Double)()
                    End If

                    For Each coOccurringEntity As String In coOccurrenceMatrix(entity).Keys
                        Dim coOccurringEntityOccurrences As Integer = coOccurrenceMatrix(entity)(coOccurringEntity)

                        Dim pmi As Double = Math.Log((coOccurringEntityOccurrences * coOccurrenceMatrix.Count) / (entityOccurrences * coOccurrenceMatrix(coOccurringEntity).Sum(Function(kv) kv.Value)), 2)
                        pmiMatrix(entity)(coOccurringEntity) = pmi
                    Next
                Next

                Return pmiMatrix
            End Function
            Public Shared Function PrintOccurrenceMatrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Double)), entityList As List(Of String)) As String
                ' Prepare the header row
                Dim headerRow As String = "|               |"

                For Each entity As String In entityList
                    If coOccurrenceMatrix.ContainsKey(entity) Then
                        headerRow &= $" [{entity}] ({coOccurrenceMatrix(entity).Count}) |"
                    End If
                Next

                Dim str As String = ""
                ' Print the header row
                Console.WriteLine(headerRow)

                str &= headerRow & vbNewLine
                ' Print the co-occurrence matrix
                For Each entity As String In coOccurrenceMatrix.Keys
                    Dim rowString As String = $"| [{entity}] ({coOccurrenceMatrix(entity).Count})        |"

                    For Each coOccurringEntity As String In entityList
                        Dim count As Integer = 0
                        If coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                            count = coOccurrenceMatrix(entity)(coOccurringEntity)
                        End If
                        rowString &= $"{count.ToString().PadLeft(7)} "
                    Next

                    Console.WriteLine(rowString)
                    str &= rowString & vbNewLine
                Next
                Return str
            End Function
            ''' <summary>
            ''' The PMI matrix measures the statistical association or co-occurrence patterns between different entities in the text. It is calculated based on the co-occurrence matrix. PMI values are used to assess how much more likely two entities are to co-occur together than they would be if their occurrences were independent of each other.
            '''
            '''  positive PMI value indicates that the two entities are likely To co-occur more often than expected by chance, suggesting a positive association between them.
            '''  PMI value Of 0 means that the two entities co-occur As often As expected by chance, suggesting no significant association.
            '''  negative PMI value indicates that the two entities are less likely To co-occur than expected by chance, suggesting a negative association Or avoidance.
            ''' </summary>
            ''' <param name="coOccurrenceMatrix"></param>
            ''' <returns></returns>
            Public Shared Function GetPM_Matrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer))) As Dictionary(Of String, Dictionary(Of String, Double))

                Dim pmiMatrix As Dictionary(Of String, Dictionary(Of String, Double)) = CalculatePMI(coOccurrenceMatrix)
                Return pmiMatrix

            End Function


        End Class
        Public Class Word2WordMatrix
            Private matrix As Dictionary(Of String, Dictionary(Of String, Integer))

            Public Sub New()
                matrix = New Dictionary(Of String, Dictionary(Of String, Integer))
            End Sub
            Public Shared Function CreateDataGridView(matrix As Dictionary(Of String, Dictionary(Of String, Double))) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Object)
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Object = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function

            Public Shared Function CreateDataGridView(matrix As Dictionary(Of String, Dictionary(Of String, Integer))) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Object)()
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Integer = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function

            Public Sub AddDocument(document As String, contextWindow As Integer)
                Dim words As String() = document.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)

                For i As Integer = 0 To words.Length - 1
                    Dim currentWord As String = words(i)

                    If Not matrix.ContainsKey(currentWord) Then
                        matrix(currentWord) = New Dictionary(Of String, Integer)()
                    End If

                    For j As Integer = Math.Max(0, i - contextWindow) To Math.Min(words.Length - 1, i + contextWindow)
                        If i <> j Then
                            Dim contextWord As String = words(j)

                            If Not matrix(currentWord).ContainsKey(contextWord) Then
                                matrix(currentWord)(contextWord) = 0
                            End If

                            matrix(currentWord)(contextWord) += 1
                        End If
                    Next
                Next
            End Sub
            Public Shared Sub Main()
                ' Fill the matrix with your data
                Dim documents As List(Of String) = New List(Of String)()
                documents.Add("This is the first document.")
                documents.Add("The second document is here.")
                documents.Add("And this is the third document.")

                Dim contextWindow As Integer = 1
                Dim matrixBuilder As New Word2WordMatrix()

                For Each document As String In documents
                    matrixBuilder.AddDocument(document, contextWindow)
                Next

                Dim wordWordMatrix As Dictionary(Of String, Dictionary(Of String, Integer)) = matrixBuilder.GetWordWordMatrix()

                ' Create the DataGridView control
                Dim dataGridView As DataGridView = Word2WordMatrix.CreateDataGridView(wordWordMatrix)

                ' Create a form and add the DataGridView to it
                Dim form As New Form()
                form.Text = "Word-Word Matrix"
                form.Size = New Size(800, 600)
                form.Controls.Add(dataGridView)

                ' Display the form
                Application.Run(form)
            End Sub
            Public Function GetWordWordMatrix() As Dictionary(Of String, Dictionary(Of String, Integer))
                Return matrix
            End Function
        End Class

    End Namespace
    Namespace UseCases
        Namespace WordEmbeddingsUseCase
            Public Class WordEmbeddingsExample
                Public Shared Sub Run()
                    ' Sample text corpus for training word embeddings.
                    Dim textCorpus As List(Of String) = New List(Of String) From
                {
                    "apple orange banana",
                    "orange banana grape",
                    "grape cherry apple",
                    "apple apple apple",
                    "cherry banana banana"
                }

                    ' Create a custom vocabulary from the text corpus.
                    Dim vocabulary As List(Of String) = textCorpus.SelectMany(Function(sentence) sentence.Split()).Distinct().ToList()

                    ' Create a WordEmbeddingsModel and train it with the text corpus.
                    Dim wordEmbeddingsModel As New HybridWordEmbeddingsModel(vocabulary)
                    wordEmbeddingsModel.Train()

                    ' Get the word vector for a specific word.
                    Dim word As String = "apple"
                    Dim wordVector As Double() = wordEmbeddingsModel.WordEmbeddings.GetVector(word)


                    ' Calculate the cosine similarity between two words.
                    Dim word1 As String = "apple"
                    Dim word2 As String = "orange"
                    Dim similarity As Double = wordEmbeddingsModel.CosineSimilarity(word1, word2, wordEmbeddingsModel.WordEmbeddings)

                    ' Display the word vector and similarity result.
                    Console.WriteLine($"Word Vector for '{word}': {String.Join(", ", wordVector)}")
                    Console.WriteLine($"Cosine Similarity between '{word1}' and '{word2}': {similarity}")
                End Sub
            End Class
        End Namespace
        Namespace WordEmbeddingsUseCase
            Public Class HybridWordEmbeddingsExample
                Public Shared Sub Run()
                    ' Sample text corpus for training word embeddings.
                    Dim textCorpus As List(Of String) = New List(Of String) From
                {
                    "apple orange banana",
                    "orange banana grape",
                    "grape cherry apple",
                    "apple apple apple",
                    "cherry banana banana"
                }

                    ' Create a custom vocabulary from the text corpus.
                    Dim vocabulary As List(Of String) = textCorpus.SelectMany(Function(sentence) sentence.Split()).Distinct().ToList()

                    ' Create the hybrid word embeddings model.
                    Dim hybridModel As New HybridWordEmbeddingsModel(vocabulary)

                    ' Train the hybrid model using the two-step training process.
                    hybridModel.Train()

                    ' Get the word vector for a specific word after training.
                    Dim word As String = "apple"
                    Dim wordVector As Double() = hybridModel.WordEmbeddings.GetVector(word)

                    ' Calculate the cosine similarity between two words after training.
                    Dim word1 As String = "apple"
                    Dim word2 As String = "orange"
                    Dim similarity As Double = hybridModel.CosineSimilarity(word1, word2, hybridModel.WordEmbeddings)

                    ' Display the word vector and similarity result after training.
                    Console.WriteLine($"Word Vector for '{word}' after training: {String.Join(", ", wordVector)}")
                    Console.WriteLine($"Cosine Similarity between '{word1}' and '{word2}' after training: {similarity}")
                End Sub
            End Class
        End Namespace
    End Namespace

End Namespace

