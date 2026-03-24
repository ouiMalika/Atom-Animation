import SwiftUI

struct ContentView: View {
    @Environment(AtomViewModel.self) private var viewModel
    @Environment(\.openImmersiveSpace) private var openImmersiveSpace
    @Environment(\.dismissImmersiveSpace) private var dismissImmersiveSpace

    @State private var immersiveOpen = false

    var body: some View {
        VStack(spacing: 24) {
            Text("Atom Animation")
                .font(.extraLargeTitle)

            if viewModel.isLoading {
                ProgressView("Loading \(viewModel.frames.count > 0 ? "\(viewModel.frames.count)" : "") frames…")
                    .padding()
            } else if viewModel.frames.isEmpty {
                Text("No frame data found.")
                    .foregroundStyle(.secondary)
            } else {
                Text("\(viewModel.frames.count) frames · \(viewModel.atomTypes.count) atoms")
                    .foregroundStyle(.secondary)

                Button(immersiveOpen ? "Close Space" : "Open in Space") {
                    Task {
                        if immersiveOpen {
                            await dismissImmersiveSpace()
                            viewModel.pause()
                            immersiveOpen = false
                        } else {
                            await openImmersiveSpace(id: "AtomSpace")
                            immersiveOpen = true
                        }
                    }
                }
                .buttonStyle(.borderedProminent)

                if immersiveOpen {
                    HStack(spacing: 20) {
                        Button(viewModel.isPlaying ? "Pause" : "Play") {
                            viewModel.isPlaying ? viewModel.pause() : viewModel.play()
                        }

                        Text("Frame \(viewModel.currentFrame + 1) / \(viewModel.frames.count)")
                            .monospacedDigit()
                            .foregroundStyle(.secondary)
                    }
                    .padding(.top, 8)
                }
            }
        }
        .padding(48)
        .task {
            await viewModel.load()
        }
    }
}
