import SwiftUI
import RealityKit

struct ImmersiveView: View {
    @Environment(AtomViewModel.self) private var viewModel

    var body: some View {
        // Accessing currentFrame here creates a SwiftUI dependency so the
        // RealityView update closure fires on every frame tick.
        let _ = viewModel.currentFrame

        RealityView { content in
            let anchor = viewModel.buildScene()
            content.add(anchor)
        } update: { _ in
            viewModel.updatePositions()
        }
        .onAppear {
            viewModel.play()
        }
        .onDisappear {
            viewModel.pause()
        }
    }
}
