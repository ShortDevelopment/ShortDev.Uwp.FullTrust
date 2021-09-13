﻿Imports VBAudioRouter.Controls
Imports Windows.Media.Audio

Namespace AudioGraphControl

    Public Class ConnectionHelper

        Public Shared Sub DeleteConnection(connection As NodeConnection)
            ' Get graph nodes
            Dim sourceNode As IAudioInputNode = DirectCast(connection.SourceConnector.AttachedNode.BaseAudioNode, IAudioInputNode)
            Dim destinationNode As IAudioNode = connection.DestinationConnector.AttachedNode.BaseAudioNode
            ' Remove graph connection
            sourceNode.RemoveOutgoingConnection(destinationNode)

            ' Remove visual
            DirectCast(connection.Line.Parent, Canvas).Children.Remove(connection.Line)
            ' Remove reference
            connection.SourceConnector.Connections.Remove(connection)
            connection.DestinationConnector.Connections.Remove(connection)
        End Sub

        Public Shared Sub DisposeNode(nodeControl As NodeControl)
            Dim node = DirectCast(nodeControl.NodeContent, IAudioNodeControl)

            ' Remove visual & graph connections
            Dim connections = node.OutgoingConnector.Connections.ToArray()
            For Each connection In connections
                DeleteConnection(connection)
            Next

            If Not node.BaseAudioNode Is Nothing Then
                ' Dispose audio node
                node.BaseAudioNode.Stop()
                node.BaseAudioNode.Dispose()
            End If

            ' Remove visual node
            DirectCast(nodeControl.Parent, Grid).Children.Remove(nodeControl)
        End Sub

    End Class

End Namespace